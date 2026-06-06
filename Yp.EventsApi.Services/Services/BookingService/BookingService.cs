using AutoMapper;
using Microsoft.Extensions.Logging;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Services.BookingService;

public class BookingService: IBookingService
{
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    private readonly IEventService _eventService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IMapper mapper, ILogger<BookingService> logger, IEventService eventService, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _logger = logger;
        _eventService = eventService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException($"Не удалось найти бронирование. Бронирование с идентификатором {bookingId} не найдено");
        }
        
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    { 
        await _eventService.TryReserveSeats(eventId, 1, cancellationToken);
        
        var booking = Booking.CreateInstance(Guid.NewGuid(), eventId, BookingStatus.Pending);
        
        await _bookingRepository.CreateAsync(booking, cancellationToken); 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<List<BookingDto>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken ct)
    {
        var bookings = await _bookingRepository.GetAllByStatusAsync(status, ct);
        
        return _mapper.Map<List<BookingDto>>(bookings);
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