using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Services.BookingService;

public interface IBookingService
{
    public Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken ct = default);

    public Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);
    
    public Task<List<BookingDto>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken ct = default);
    
    public Task ConfirmBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default);
    
    public Task RejectBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct = default);
}