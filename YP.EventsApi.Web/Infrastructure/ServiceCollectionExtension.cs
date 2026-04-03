using System.Reflection;
using FluentValidation;
using YP.EventApi.Web.Validators;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Shared.Contracts;

namespace YP.EventApi.Web.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Регистрируем как синглтон, чтобы хранить данные в инстансе сервиса. При переходе на внешний источник данных переделать на Scoped
        services.AddSingleton<IEventService, EventService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        services.AddScoped<IValidator<EventCreateDto>, EventCreateDtoValidator>();

        return services;
    }

    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
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