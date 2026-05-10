using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Yp.EventsApi.Services.DataAccess;
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
    private readonly AppDbContext _context;

    public EventService(IMapper mapper, AppDbContext context)
    {
        // _events = new()
        // {
        //     Event.CreateInstance(Guid.NewGuid(), "Тренировка по боксу", new DateTime(2025, 03, 20 ), new DateTime(2025, 04, 20), 10),
        //     Event.CreateInstance(Guid.NewGuid(), "День рождения", new DateTime(2024, 03, 20 ), new DateTime(2026, 03, 20), 25),
        //     Event.CreateInstance(Guid.NewGuid(), "Корпоратив", new DateTime(2023, 03, 20 ), new DateTime(2024, 03, 20), 15),
        //     Event.CreateInstance(Guid.NewGuid(), "Поездка на море", new DateTime(2026, 04, 20 ), new DateTime(2026, 05, 13), 20),
        //     Event.CreateInstance(Guid.NewGuid(), "Свадьба", new DateTime(2026, 06, 10 ), new DateTime(2026, 06, 10), 5)
        // };
        _context = context;
        _mapper = mapper;
    }

    
    public PaginatedResult<EventDto> GetAll(EventFilter filter)
    {
        var query = _context.Events.AsQueryable();

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
    
    public async Task<EventDto> GetById(Guid id)
    {
        var result = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (result == null)
        {
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {id} не найдено");
        }
        
        return _mapper.Map<EventDto>(result);
    }

    public async Task<EventDto> Create(EventCreateDto eventCreateDto)
    {
        var newEvent = Event.CreateInstance(Guid.NewGuid(), eventCreateDto.Title, eventCreateDto.StartAt, eventCreateDto.EndAt, eventCreateDto.TotalSeats, eventCreateDto.Description);
        
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return _mapper.Map<EventDto>(newEvent);
    }

    public async Task<EventDto> Update(Guid eventId, EventCreateDto eventCreateDto)
    {
        var existingEvent = _context.Events.FirstOrDefault(e => e.Id == eventId);

        if (existingEvent == null)
        {
            throw new EntityNotFoundException($"Не удалось обновить событие. Событие с идентификатором {eventId} не найдено");
        }

        existingEvent = _mapper.Map<Event>(eventCreateDto);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<EventDto>(existingEvent);
    }


    public async Task<bool> TryReserveSeats(Guid eventId, int seatsCount = 1)
    {
        var currentEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        
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

    public async Task<bool> ReleaseSeats(Guid eventId, int seatsCount = 1)
    {
        if (seatsCount <= 0)
        {
            return false;
        }
        
        var currentEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        
        if (currentEvent == null)
        {
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {eventId} не найдено");
        }

        currentEvent.ReleaseSeats(seatsCount);

        return true;
    }

    public async Task Delete(Guid eventId)
    {
        var eventToDelete = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventToDelete == null)
        {
            throw new EntityNotFoundException($"Не удалось удалить событие. Событие с идентификатором {eventId} не найдено");
        }
        
        _context.Events.Remove(eventToDelete);
        await _context.SaveChangesAsync();
    }
}