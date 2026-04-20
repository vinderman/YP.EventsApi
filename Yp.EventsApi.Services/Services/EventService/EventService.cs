using AutoMapper;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Services.Services.EventService;

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
            Event.CreateInstance(Guid.NewGuid(), "Тренировка по боксу", new DateTime(2025, 03, 20 ), new DateTime(2025, 04, 20), 10),
            Event.CreateInstance(Guid.NewGuid(), "День рождения", new DateTime(2024, 03, 20 ), new DateTime(2026, 03, 20), 25),
            Event.CreateInstance(Guid.NewGuid(), "Корпоратив", new DateTime(2023, 03, 20 ), new DateTime(2024, 03, 20), 15),
            Event.CreateInstance(Guid.NewGuid(), "Поездка на море", new DateTime(2026, 04, 20 ), new DateTime(2026, 05, 13), 20),
            Event.CreateInstance(Guid.NewGuid(), "Свадьба", new DateTime(2026, 06, 10 ), new DateTime(2026, 06, 10), 5)
        };
        
        _mapper = mapper;
    }

    
    public PaginatedResult<EventDto> GetAll(EventFilter filter)
    {
        var query = _events.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            query = query.Where(e => e.Title.Contains(filter.Title, StringComparison.InvariantCultureIgnoreCase));
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
        var newEvent = Event.CreateInstance(Guid.NewGuid(), eventCreateDto.Title, eventCreateDto.StartAt, eventCreateDto.EndAt, eventCreateDto.TotalSeats, eventCreateDto.Description);
        
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


    public bool TryReserveSeats(Guid eventId, int seatsCount = 1)
    {
        var currentEvent = _events.FirstOrDefault(e => e.Id == eventId);
        
        if (currentEvent == null)
        {
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {eventId} не найдено");
        }

        var isReserved = currentEvent.TryReserveSeats(seatsCount);
        
        if (!isReserved)
        {
            throw new NoAvailableSeatsException("Для данного события нет доступных мест");
        }

        return true;
    }

    public bool ReleaseSeats(Guid eventId, int seatsCount = 1)
    {
        var currentEvent = _events.FirstOrDefault(e => e.Id == eventId);
        
        if (currentEvent == null)
        {
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {eventId} не найдено");
        }

        var isReserved = currentEvent.TryReserveSeats(seatsCount);
        
        if (!isReserved)
        {
            throw new NoAvailableSeatsException("Для данного события нет доступных мест");
        }

        return true;
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