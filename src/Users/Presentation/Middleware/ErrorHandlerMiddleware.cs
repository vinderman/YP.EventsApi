using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;

namespace Presentation.Middleware;

public class ErrorHandlerMiddleware
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
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
            EntityNotFoundException => StatusCodes.Status404NotFound,
            DomainValidationException => StatusCodes.Status400BadRequest,
            ForbiddenException => StatusCodes.Status403Forbidden,
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
