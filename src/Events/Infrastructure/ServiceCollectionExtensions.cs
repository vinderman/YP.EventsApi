using Application.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Options;
using Shared.UnitOfWork;
using StackExchange.Redis;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        services.Configure<KafkaSettings>(configuration.GetSection(nameof(KafkaSettings)));
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddHostedService<ConfirmBookingConsumer>();

        var redisSettings = configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
        
        var options = new ConfigurationOptions
        {
            EndPoints = { redisSettings.Server },
            Password = redisSettings.Password,
            ConnectTimeout = 5000,
            SyncTimeout = 3000,
            AbortOnConnectFail = false,
        };
        
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(options)
        );

        services.AddSingleton<ICacheService, RedisCache>();

        return services;
    }

    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEventRepository, EventRepository>();
        return services;
    }
}
