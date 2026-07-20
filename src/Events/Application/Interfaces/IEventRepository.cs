using Application.Models;
using Domain.Entities;

namespace Application.Interfaces;

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

    void Remove(Event entity);
    
    /// <summary>
    /// Возвращает n событий с самым высоким процентом выкупленных мест
    /// </summary>
    /// <param name="count"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<Event>> GetTopSelledEvents(int count, CancellationToken cancellationToken);
}
