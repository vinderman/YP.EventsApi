using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Services.Services.EventService;

public interface IEventService
{
    /// <summary>
    /// Получить все события
    /// </summary>
    PaginatedResult<EventDto> GetAll(EventFilter filter);
    
    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="eventId">Идентификатор</param>
    EventDto GetById(Guid eventId);
    
    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="newEvent"></param>
    EventDto Create(EventCreateDto newEvent);
    
    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="eventToUpdate"></param>
    EventDto Update(Guid eventId, EventCreateDto eventToUpdate);

    /// <summary>
    /// Попытаться зарезервировать места на события
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="seatsCount"></param>
    /// <returns></returns>
    bool TryReserveSeats(Guid eventId, int seatsCount = 1);
    
    /// <summary>
    /// Освободить места на события
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="seatsCount"></param>
    /// <returns></returns>
    bool ReleaseSeats(Guid eventId, int seatsCount = 1);
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="eventId"></param>
    void Delete(Guid eventId);
}