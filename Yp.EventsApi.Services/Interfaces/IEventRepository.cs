using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Services.Interfaces;

public interface IEventRepository
{
    Task<(IReadOnlyList<Event> Items, int Total)> GetPagedAsync(
        EventFilter filter,
        CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Загружает событие с блокировкой строки (<c>SELECT … FOR UPDATE</c>) в рамках активной транзакции контекста.
    /// </summary>
    Task<Event?> GetByIdForUpdateAsync(Guid eventId, CancellationToken cancellationToken);
    
    Task AddAsync(Event createEvent, CancellationToken cancellationToken);
    
    void Remove(Guid eventId);
}