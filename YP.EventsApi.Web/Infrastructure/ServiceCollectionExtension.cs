using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using YP.EventApi.Web.Validators;
using Yp.EventsApi.Services.DataAccess;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Services.Services.BackgroundServices;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;

namespace YP.EventApi.Web.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddHostedService<BookingProcessorBackgroundService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        services.AddScoped<IValidator<EventCreateDto>, EventCreateDtoValidator>();

        return services;
    }

    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
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