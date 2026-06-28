using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Infrastructure.Options;
using Yp.EventsApi.Infrastructure.Repositories;
using Yp.EventsApi.Infrastructure.Security;

namespace Yp.EventsApi.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


        return services;
    }
    
    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}