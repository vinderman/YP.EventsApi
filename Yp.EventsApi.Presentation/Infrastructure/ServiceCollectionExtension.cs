using FluentValidation;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.BackgroundServices;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Infrastructure.Repositories;
using Yp.EventsApi.Presentation.Contracts;
using Yp.EventsApi.Presentation.Validators;

namespace Yp.EventsApi.Presentation.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        services.AddScoped<IValidator<EventCreateDto>, EventCreateDtoValidator>();
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {   
            var xmlDocumentationFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            xmlDocumentationFiles.ForEach(xmlDocumentationFile => options.IncludeXmlComments(xmlDocumentationFile));
            
            options.SupportNonNullableReferenceTypes();
        });

        return services;
    }
}