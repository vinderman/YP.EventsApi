using Microsoft.Extensions.Logging;
using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;
using Yp.EventsApi.Domain.Exceptions;

namespace Yp.EventsApi.Application.Services.BookingService;

public class BookingService : IBookingService
{
    private readonly ILogger<BookingService> _logger;
    private readonly IEventService _eventService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(ILogger<BookingService> logger, IEventService eventService,
        IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
        _eventService = eventService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException(
                $"Не удалось найти бронирование. Бронирование с идентификатором {bookingId} не найдено");
        }
        
        EnsureCanAccessBookingAsync(booking, userId, role, cancellationToken);

        return booking;
    }

    public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanCreateBookingAsync(eventId, userId, cancellationToken);

        await _eventService.TryReserveSeats(eventId, 1, cancellationToken);

        var booking = Booking.CreateInstance(Guid.NewGuid(), eventId, BookingStatus.Pending, userId);

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

    public async Task CancelBookingAsync(Guid bookingId, Guid userId, UserRole role, CancellationToken ct = default)
    {
        var booking = await EnsureBookingExists(bookingId, ct);
        
        EnsureCanAccessBookingAsync(booking, userId, role, ct);

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new DomainValidationException("Нельзя повторно отменить бронирование");
        }

        await _eventService.ReleaseSeats(booking.EventId, 1, ct);
        booking.Status = BookingStatus.Cancelled;
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

    private async Task EnsureCanCreateBookingAsync(Guid eventId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userBookingsCount = await _bookingRepository.CountActiveByUserIdAsync(userId, cancellationToken);
        if (userBookingsCount >= 10)
        {
            throw new BookingCountExceededException();
        }
        
        var eventEntity = await _eventService.GetById(eventId, cancellationToken);
        eventEntity.EnsureCanAcceptBooking(DateTime.Now);
    }

    private static void EnsureCanAccessBookingAsync(Booking booking, Guid userId, UserRole role,
        CancellationToken cancellationToken = default)
    {
        if (role == UserRole.Admin)
            return;
        if (booking.UserId != userId)
            throw new ForbiddenException("Недостаточно прав для доступа к бронированию");
    }
}