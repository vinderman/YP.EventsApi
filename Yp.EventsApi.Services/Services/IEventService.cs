using Yp.EventsApi.Services.Entities;

namespace Yp.EventsApi.Services.Services;

public interface IEventService
{
    /// <summary>
    /// Получить все события
    /// </summary>
    IEnumerable<Event> GetAll();
    
    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="eventId">Идентификатор</param>
    Event? GetById(Guid eventId);
    
    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="newEvent"></param>
    Event Create(Event newEvent);
    
    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="updatedEvent"></param>
    Event Update(Event updatedEvent);
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="eventId"></param>
    void Delete(Guid eventId);
}