using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Application.Interfaces;

public interface IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    
    public Task CreateAsync(Booking booking, CancellationToken cancellationToken = default);
    
    public Task<IReadOnlyList<Booking>> GetAllByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Подсчитать количество активных бронирований у пользователя
    /// </summary>
    public Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}