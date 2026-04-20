using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Services.BackgroundServices;

public class BookingProcessorBackgroundService: BackgroundService
{
    private readonly ILogger<BookingProcessorBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    
    public BookingProcessorBackgroundService(ILogger<BookingProcessorBackgroundService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Штатная остановка, выходим из цикла
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Возникла ошибка при обработке бронирования");
            }
            
            await Task.Delay(10_000, stoppingToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        
        var pendingBookings = await bookingService.GetBookingsByStatusAsync(BookingStatus.Pending, stoppingToken);

        var tasks = pendingBookings.Select(booking => ProcessSingleBooking(booking, stoppingToken));
       
        await Task.WhenAll(tasks);
    }

    private async Task ProcessSingleBooking(BookingDto booking, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        try
        {
            // убеждаемся, что событие существует
            eventService.GetById(booking.EventId);
        }
        catch (EntityNotFoundException e)
        {
            _logger.LogWarning(e, "Событие не найдено, отклоняем бронирование {Id}", booking.Id);
            
            // Если произошла ошибка, отклоняем бронь
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                await bookingService.RejectBookingAsync(booking.Id, booking.EventId, stoppingToken);
            }
            finally
            {
                _semaphore.Release();
            }
            
            return;
        }

        try
        {
            
            await Task.Delay(2000, stoppingToken);
            await bookingService.ConfirmBookingAsync(booking.Id, booking.EventId, stoppingToken);
            _logger.LogInformation("Бронирование {Id} успешно обработано", booking.Id);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Произошла ошибка при обработке бронирования {id}", booking.Id);
            
            // Если произошла ошибка, отклоняем бронь
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                await bookingService.RejectBookingAsync(booking.Id, booking.EventId, stoppingToken);
            }
            finally
            {
                _semaphore.Release();
            }
            
        }
    }
}