using Yp.EventsApi.Application.Models;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Application.Services.EventService;

public interface IEventService
{
    /// <summary>
    /// Получить все события
    /// </summary>
    Task<PaginatedResult<Event>> GetAll(EventFilter filter, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="eventId">Идентификатор</param>
    Task<Event> GetById(Guid eventId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="newEventRequest"></param>
    Task<Event> Create(CreateEventRequest newEventRequest, CancellationToken cancellationToken);
    
    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="eventRequestToUpdate"></param>
    Task<Event> Update(Guid eventId, UpdateEventRequest eventRequestToUpdate, CancellationToken cancellationToken);

    /// <summary>
    /// Попытаться зарезервировать места на события
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="seatsCount"></param>
    /// <returns></returns>
    Task<bool> TryReserveSeats(Guid eventId, int seatsCount, CancellationToken cancellationToken);
    
    /// <summary>
    /// Освободить места на события
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="seatsCount"></param>
    /// <returns></returns>
    Task<bool> ReleaseSeats(Guid eventId, int seatsCount, CancellationToken cancellationToken);
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="eventId"></param>
    Task Delete(Guid eventId, CancellationToken cancellationToken);
}