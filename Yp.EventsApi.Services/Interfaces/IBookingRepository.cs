using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Services.Interfaces;

public interface IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    
    public Task CreateAsync(Booking booking, CancellationToken cancellationToken = default);
    
    public Task<IReadOnlyList<Booking>> GetAllByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default);
}