using AutoMapper;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Services.Services.EventService;

/// <summary>
/// Сервис обработки событий
/// </summary>
public class EventService: IEventService
{
    private readonly IMapper _mapper;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IMapper mapper, IEventRepository eventRepository, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    
    public async Task<PaginatedResult<EventDto>> GetAll(EventFilter filter, CancellationToken cancellationToken = default)
    {
        var (events, totalCount) = await _eventRepository.GetPagedAsync(filter, cancellationToken);
        
        return new PaginatedResult<EventDto>
        {
            Total = totalCount,
            CurrentPage = filter.Page,
            Items = _mapper.Map<List<EventDto>>(events),
            PageSize = filter.PageSize
            
        };
    }
    
    public async Task<EventDto> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _eventRepository.GetByIdAsync(id, cancellationToken);

        if (result == null)
        {
            throw new EntityNotFoundException($"Не удалось найти событие. Событие с идентификатором {id} не найдено");
        }
        
        return _mapper.Map<EventDto>(result);
    }

    public async Task<EventDto> Create(EventCreateDto eventCreateDto, CancellationToken cancellationToken)
    {
        var newEvent = Event.CreateInstance(Guid.NewGuid(), eventCreateDto.Title, eventCreateDto.StartAt, eventCreateDto.EndAt, eventCreateDto.TotalSeats, eventCreateDto.Description);
        
        await _eventRepository.AddAsync(newEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<EventDto>(newEvent);
    }

    public async Task<EventDto> Update(Guid eventId, EventCreateDto eventCreateDto, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(eventId, cancellationToken);

        if (existingEvent == null)
        {
            throw new EntityNotFoundException($"Не удалось обновить событие. Событие с идентификатором {eventId} не найдено");
        }

        existingEvent = _mapper.Map<Event>(eventCreateDto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<EventDto>(existingEvent);
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