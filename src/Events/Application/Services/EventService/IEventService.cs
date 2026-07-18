using Application.Models;
using Domain.Entities;

namespace Application.Services.EventService;

public interface IEventService
{
    /// <summary>
    /// Получить все события
    /// </summary>
    Task<PaginatedResult<Event>> GetAll(EventFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    Task<Event> GetById(Guid eventId, CancellationToken cancellationToken);

    /// <summary>
    /// Создать новое событие
    /// </summary>
    Task<Event> Create(CreateEventRequest newEventRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить событие
    /// </summary>
    Task<Event> Update(Guid eventId, UpdateEventRequest eventRequestToUpdate, CancellationToken cancellationToken);

    /// <summary>
    /// Попытаться зарезервировать места на событии
    /// </summary>
    Task<bool> TryReserveSeats(Guid eventId, int seatsCount, CancellationToken cancellationToken);

    /// <summary>
    /// Освободить места на событии
    /// </summary>
    Task<bool> ReleaseSeats(Guid eventId, int seatsCount, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить событие
    /// </summary>
    Task Delete(Guid eventId, CancellationToken cancellationToken);
}
