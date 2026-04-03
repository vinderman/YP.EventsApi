using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Services.BookingService;

public interface IBookingService
{
    public Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken ct);

    public Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);
    
    public Task<List<BookingDto>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken ct);
    
    public Task UpdateBookingStatusAsync(UpdateBookingStatusRequest updateBookingStatusRequest, CancellationToken ct);
}