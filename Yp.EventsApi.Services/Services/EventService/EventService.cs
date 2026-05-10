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
        _context = context;
        _mapper = mapper;
    }

    
    public async Task<PaginatedResult<EventDto>> GetAll(EventFilter filter)
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
        
        var events = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        
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
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var currentEvent = await LoadEventForUpdateAsync(eventId);

        if (currentEvent == null)
        {
            await transaction.RollbackAsync();
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {eventId} не найдено");
        }

        var isReserved = currentEvent.TryReserveSeats(seatsCount);

        if (!isReserved)
        {
            await transaction.RollbackAsync();
            throw new NoAvailableSeatsException("Для данного события нет доступных мест");
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    public async Task<bool> ReleaseSeats(Guid eventId, int seatsCount = 1)
    {
        if (seatsCount <= 0)
        {
            return false;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var currentEvent = await LoadEventForUpdateAsync(eventId);

        if (currentEvent == null)
        {
            await transaction.RollbackAsync();
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {eventId} не найдено");
        }

        currentEvent.ReleaseSeats(seatsCount);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    /// <summary>
    /// Загружает событие с блокировкой строки (<c>SELECT … FOR UPDATE</c>) в рамках активной транзакции контекста.
    /// </summary>
    private async Task<Event?> LoadEventForUpdateAsync(Guid eventId)
    {
        return await _context.Events.FromSqlInterpolated($"SELECT * FROM \"Events\" WHERE \"Id\" = {eventId} FOR UPDATE").FirstOrDefaultAsync();
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