using System.Collections.Concurrent;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yp.EventsApi.Services.DataAccess;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Services.BookingService;

public class BookingService: IBookingService
{
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    private readonly IEventService _eventService;
    private readonly AppDbContext _context;

    public BookingService(IMapper mapper, ILogger<BookingService> logger, IEventService eventService, AppDbContext context)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _eventService = eventService;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking == null)
        {
            throw new EntityNotFoundException($"Не удалось найти бронирование. Бронирование с идентификатором {bookingId} не найдено");
        }
        
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    { 
        await _eventService.TryReserveSeats(eventId);
        
        var booking = Booking.CreateInstance(Guid.NewGuid(), eventId, BookingStatus.Pending);
        
        _context.Bookings.Add(booking); 
        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<List<BookingDto>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken ct)
    {
        var bookings = await _context.Bookings.Where(b => b.Status == status).ToListAsync(ct);
        
        return _mapper.Map<List<BookingDto>>(bookings);
    }

    public async Task ConfirmBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default)
    {
        var booking = await EnsureBookingExists(bookingId);
        
        booking.Status = BookingStatus.Confirmed;
        booking.ProcessedAt = DateTime.Now;
        
        await _context.SaveChangesAsync(ct);
    }

    public async Task RejectBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default)
    {
        var booking = await EnsureBookingExists(bookingId);
        
        await _eventService.ReleaseSeats(eventId);
        booking.Status = BookingStatus.Rejected;
        booking.ProcessedAt = DateTime.Now;
        
        await _context.SaveChangesAsync(ct);
    }

    private async Task<Booking> EnsureBookingExists(Guid bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

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