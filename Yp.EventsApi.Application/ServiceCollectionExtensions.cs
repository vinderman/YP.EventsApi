using Microsoft.Extensions.DependencyInjection;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.BackgroundServices;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Application.Services.UserService;

namespace Yp.EventsApi.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddHostedService<BookingProcessorBackgroundService>();
        return services;
    }
}