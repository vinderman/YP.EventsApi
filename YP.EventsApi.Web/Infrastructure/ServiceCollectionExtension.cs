using System.Reflection;
using Yp.EventsApi.Services.Services;

namespace YP.EventApi.Web.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

        return services;
    }

    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {   
            // Путь к XML-файлу с документацией
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}