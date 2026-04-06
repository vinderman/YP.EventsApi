using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Yp.EventsApi.Services.Exceptions;

namespace YP.EventApi.Web.Middleware;

public class ErrorHandlerMiddleware: ControllerBase
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    
    
    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    /// <summary>
    /// Ошибки валидации обрабатываем отдельно, маппим их к формату ProblemDetails
    /// </summary>
    /// <param name="context"></param>
    /// <param name="exception"></param>
    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        var mappedErrors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        
        var error  = new ValidationProblemDetails(mappedErrors)
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message
        };
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(error);
       
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Произошла ошибка. Метод: {Method}. Путь: {Path}.", context.Request.Method, context.Request.Path);
        
        if (context.Response.HasStarted)
        {
            return;
        }

        var statusCode = exception switch
        {
            EntityNotFoundException en => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var error = new ProblemDetails
        {
            Title = exception.Message,
            Status = statusCode
        };
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(error);
    }
}