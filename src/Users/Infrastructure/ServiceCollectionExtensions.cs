using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Options;
using Shared.UnitOfWork;

namespace Infrastructure;

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
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}