using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Application.Services.BookingService;

public interface IBookingService
{
    public Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    public Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);
    
    public Task<List<Booking>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken ct = default);
    
    public Task ConfirmBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default);
    
    public Task RejectBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default);

    public Task CancelBookingAsync(Guid bookingId, Guid userId, UserRole role, CancellationToken ct = default);
}