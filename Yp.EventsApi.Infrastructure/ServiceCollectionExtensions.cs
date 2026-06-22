using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Infrastructure.Repositories;

namespace Yp.EventsApi.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
    
    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        return services;
    }
}