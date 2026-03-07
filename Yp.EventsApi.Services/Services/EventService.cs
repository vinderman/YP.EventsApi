using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Yp.EventsApi.Services.Entities;

namespace Yp.EventsApi.Services.Services;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class EventService: IEventService
{
    private readonly IMapper _mapper;
    private List<Event> _events = new ();

    public EventService(IMapper mapper)
    {
        _mapper = mapper;
    }

    
    public IEnumerable<Event> GetAll()
    {
        return _events.AsEnumerable();
    }
    
    public Event? GetById(Guid id)
    {
        return _events.Find(e => e.Id == id);
    }

    public Event Create(Event newEvent)
    {
        newEvent.Id = Guid.NewGuid();
        
        _events.Add(newEvent);
        return newEvent;
    }

    public Event? Update(Event updatedEvent)
    {
        var updateAtIndex = _events.FindIndex(e => e.Id == updatedEvent.Id);
        if (updateAtIndex != -1)
        {
            _events[updateAtIndex] = updatedEvent;
            return updatedEvent;
        }

        return null;
    }

    public void Delete(Guid eventId)
    {
        var eventToDelete = _events.Find(e => e.Id == eventId);

        if (eventToDelete == null)
        {
            throw new Exception("Событие не найдено");
        }
        
        _events.Remove(eventToDelete);
    }
}