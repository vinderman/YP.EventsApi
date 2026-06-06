using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Models;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Application.Services.EventService;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class EventService: IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IEventRepository eventRepository, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    
    public async Task<PaginatedResult<Event>> GetAll(EventFilter filter, CancellationToken cancellationToken = default)
    {
        var (events, totalCount) = await _eventRepository.GetPagedAsync(filter, cancellationToken);
        
        return new PaginatedResult<Event>
        {
            Total = totalCount,
            CurrentPage = filter.Page,
            Items = events,
            PageSize = filter.PageSize
            
        };
    }
    
    public async Task<Event> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _eventRepository.GetByIdAsync(id, cancellationToken);

        if (result == null)
        {
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {id} не найдено");
        }
        
        return result;
    }

    public async Task<Event> Create(CreateEventRequest eventCreateRequest, CancellationToken cancellationToken)
    {
        var newEvent = Event.CreateInstance(Guid.NewGuid(), eventCreateRequest.Title, eventCreateRequest.StartAt, eventCreateRequest.EndAt, eventCreateRequest.TotalSeats, eventCreateRequest.Description);
        
        await _eventRepository.AddAsync(newEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return newEvent;
    }

    public async Task<Event> Update(Guid eventId, UpdateEventRequest eventUpdateRequest, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (existingEvent == null)
        {
            throw new EntityNotFoundException($"Не удалось обновить событие. Событие с идентификатором {eventId} не найдено");
        }
        
        // TODO: изменять через Domain
        existingEvent.Title = eventUpdateRequest.Title;
        existingEvent.StartAt = eventUpdateRequest.StartAt;
        existingEvent.EndAt = eventUpdateRequest.EndAt;
        existingEvent.TotalSeats = eventUpdateRequest.TotalSeats;
        existingEvent.Description = eventUpdateRequest.Description;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return existingEvent;
    }


    public async Task<bool> TryReserveSeats(Guid eventId, int seatsCount = 1, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var currentEvent = await _eventRepository.GetByIdForUpdateAsync(eventId, cancellationToken);

        if (currentEvent == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {eventId} не найдено");
        }

        var isReserved = currentEvent.TryReserveSeats(seatsCount);

        if (!isReserved)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new NoAvailableSeatsException("Для данного события нет доступных мест");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ReleaseSeats(Guid eventId, int seatsCount = 1, CancellationToken cancellationToken = default)
    {
        if (seatsCount <= 0)
        {
            return false;
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var currentEvent = await _eventRepository.GetByIdForUpdateAsync(eventId, cancellationToken);

        if (currentEvent == null)
        {
            await transaction.RollbackAsync();
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {eventId} не найдено");
        }

        currentEvent.ReleaseSeats(seatsCount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task Delete(Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventToDelete = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (eventToDelete == null)
        {
            throw new EntityNotFoundException($"Не удалось удалить событие. Событие с идентификатором {eventId} не найдено");
        }
        
        _eventRepository.Remove(eventToDelete.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}