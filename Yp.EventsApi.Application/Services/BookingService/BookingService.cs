using Microsoft.Extensions.Logging;
using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Application.Services.BookingService;

public class BookingService: IBookingService
{
    private readonly ILogger<BookingService> _logger;
    private readonly IEventService _eventService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(ILogger<BookingService> logger, IEventService eventService, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
        _eventService = eventService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException($"Не удалось найти бронирование. Бронирование с идентификатором {bookingId} не найдено");
        }
        
        return booking;
    }

    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    { 
        await _eventService.TryReserveSeats(eventId, 1, cancellationToken);
        
        var booking = Booking.CreateInstance(Guid.NewGuid(), eventId, BookingStatus.Pending);
        
        await _bookingRepository.CreateAsync(booking, cancellationToken); 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return booking;
    }

    public async Task<List<Booking>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken ct)
    {
        var bookings = await _bookingRepository.GetAllByStatusAsync(status, ct);
        
        return bookings.ToList();
    }

    public async Task ConfirmBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default)
    {
        var booking = await EnsureBookingExists(bookingId, ct);
        
        booking.Status = BookingStatus.Confirmed;
        booking.ProcessedAt = DateTime.UtcNow;
        
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RejectBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default)
    {
        var booking = await EnsureBookingExists(bookingId);
        
        await _eventService.ReleaseSeats(eventId, 1, ct);
        booking.Status = BookingStatus.Rejected;
        booking.ProcessedAt = DateTime.UtcNow;
        
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Booking> EnsureBookingExists(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

        if (booking == null)
        {
            var message =
                $"Не удалось обновить статус бронирования. Бронирование с идентификатором {bookingId} не найдено";
            _logger.LogError(message);
            throw new EntityNotFoundException(message);
        }
        
        return booking;
    }
}