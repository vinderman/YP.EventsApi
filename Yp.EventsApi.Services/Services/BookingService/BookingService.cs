using AutoMapper;
using Microsoft.Extensions.Logging;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Services.BookingService;

public class BookingService: IBookingService
{
    private readonly IMapper _mapper;
    private List<Booking> _bookings;
    private readonly ILogger<BookingService> _logger;
    private readonly IEventService _eventService;

    public BookingService(IMapper mapper, ILogger<BookingService> logger, IEventService eventService)
    {
        _bookings = new();
        _mapper = mapper;
        _logger = logger;
        _eventService = eventService;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new EntityNotFoundException($"Не удалось найти бронирование. Бронирование с идентификатором {bookingId} не найдено");
        }
        
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var existingEvent = _eventService.GetById(eventId);
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = existingEvent.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
        };
        
        _bookings.Add(booking);
        
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<List<BookingDto>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken ct)
    {
        var bookings = _bookings.Where(b => b.Status == status);
        
        return _mapper.Map<List<BookingDto>>(bookings);
    }

    public async Task UpdateBookingStatusAsync(UpdateBookingStatusRequest updateBookingStatusRequest,
        CancellationToken ct = default)
    {
        var bookingIndex = _bookings.FindIndex(b => b.Id == updateBookingStatusRequest.Id);

        if (bookingIndex == -1)
        {
            var message =
                $"Не удалось обновить статус бронирования. Бронирование с идентификатором {updateBookingStatusRequest.Id} не найдено";
            _logger.LogError(message);
            throw new EntityNotFoundException(message);
        }
        
        var booking = _bookings[bookingIndex];
        
        booking.Status = updateBookingStatusRequest.Status;
        if (updateBookingStatusRequest.Status == BookingStatus.Confirmed)
        {
            booking.ProcessedAt = DateTime.Now;
        }
    }
}