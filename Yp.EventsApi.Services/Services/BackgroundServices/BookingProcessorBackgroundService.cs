using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Services.BackgroundServices;

public class BookingProcessorBackgroundService: BackgroundService
{
    private readonly ILogger<BookingProcessorBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    
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
                _logger.LogError(e.Message, "Возникла ошибка при обработке бронирования");
            }
            
            await Task.Delay(10_000, stoppingToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        
        var pendingBookings = await bookingService.GetBookingsByStatusAsync(BookingStatus.Pending, stoppingToken);

        if (pendingBookings.Any())
        {
            foreach (var booking in pendingBookings)
            {
                await ProcessSingleBooking(booking, bookingService, stoppingToken);
            }
        }
    }

    private async Task ProcessSingleBooking(BookingDto booking, IBookingService bookingService, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(2000, stoppingToken);
            await bookingService.UpdateBookingStatusAsync(new UpdateBookingStatusRequest
            {
                Id = booking.Id,
                Status = BookingStatus.Confirmed,
            }, stoppingToken);
                
            _logger.LogInformation("Бронирование {Id} успешно обработано", booking.Id);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Произошла ошибка при обработке бронирования {id}", booking.Id);
        }
       
    }
}