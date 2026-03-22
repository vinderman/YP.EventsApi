using AutoMapper;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Services.Services;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class EventService: IEventService
{
    private readonly IMapper _mapper;
    private List<Event> _events;

    public EventService(IMapper mapper)
    {
        _events = new()
        {
            new Event { Id = Guid.NewGuid(), Title = "Тренировка по боксу", StartAt = new DateTime(2025, 03, 20 ), EndAt = new DateTime(2025, 04, 20) },
            new Event { Id = Guid.NewGuid(), Title = "День рождения", StartAt = new DateTime(2024, 03, 20 ), EndAt = new DateTime(2026, 03, 20) },
            new Event { Id = Guid.NewGuid(), Title = "Корпоратив", StartAt = new DateTime(2023, 03, 20 ), EndAt = new DateTime(2024, 03, 20) },
            new Event { Id = Guid.NewGuid(), Title = "Поездка на море", StartAt = new DateTime(2026, 04, 20 ), EndAt = new DateTime(2026, 05, 13) },
            new Event { Id = Guid.NewGuid(), Title = "Свадьба", StartAt = new DateTime(2026, 06, 10 ), EndAt = new DateTime(2026, 06, 10) }
        };
        
        _mapper = mapper;
    }

    
    public PaginatedResult<EventDto> GetAll(EventFilter filter)
    {
        var query = _events.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            var title = filter.Title.ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(title));
        }

        if (filter.From.HasValue)
        {
            query = query.Where(e => e.StartAt >= filter.From);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(e => e.EndAt <= filter.To);
        }
        
        var totalCount = query.Count();
        
        var events = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
        
        return new PaginatedResult<EventDto>
        {
            Total = totalCount,
            CurrentPage = filter.Page,
            Items = _mapper.Map<List<EventDto>>(events),
            PageSize = filter.PageSize
            
        };
    }
    
    public EventDto GetById(Guid id)
    {
        var result = _events.FirstOrDefault(e => e.Id == id);

        if (result == null)
        {
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {id} не найдено");
        }
        
        return _mapper.Map<EventDto>(result);
    }

    public EventDto Create(EventCreateDto eventCreateDto)
    {
        var newEvent = _mapper.Map<Event>(eventCreateDto);
        newEvent.Id = Guid.NewGuid();
        
        _events.Add(newEvent);
        return _mapper.Map<EventDto>(newEvent);
    }

    public EventDto Update(Guid eventId, EventCreateDto eventCreateDto)
    {
        var updateAtIndex = _events.FindIndex(e => e.Id == eventId);

        if (updateAtIndex == -1)
        {
            throw new EntityNotFoundException($"Не удалось обновить событие. Событие с идентификатором {eventId} не найдено");
        }
        
        var updatedEvent = _mapper.Map<Event>(eventCreateDto);
        updatedEvent.Id = eventId;


        _events[updateAtIndex] = updatedEvent;
        return _mapper.Map<EventDto>(updatedEvent);
    }

    public void Delete(Guid eventId)
    {
        var eventToDelete = _events.Find(e => e.Id == eventId);

        if (eventToDelete == null)
        {
            throw new EntityNotFoundException($"Не удалось удалить событие. Событие с идентификатором {eventId} не найдено");
        }
        
        _events.Remove(eventToDelete);
    }
}