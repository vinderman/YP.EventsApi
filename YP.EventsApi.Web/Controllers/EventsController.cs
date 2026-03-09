using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using YP.EventApi.Web.Contracts;
using Yp.EventsApi.Services.Dto;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Services;

namespace YP.EventApi.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController: ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IMapper _mapper;
    
    public EventsController(IEventService eventService, IMapper mapper)
    {
        _eventService = eventService;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<EventDto>> GetEvents()
    {
        var events = _eventService.GetAll();
        return Ok(_mapper.Map<IEnumerable<EventDto>>(events));
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventDto> GetEventById(Guid id)
    {
        var eventById = _eventService.GetById(id);

        if (eventById == null)
        {
            return NotFound();
        }
        
        return Ok(_mapper.Map<EventDto>(eventById));
    }
    
    /// <summary>
    /// Создать событие
    /// </summary>
    /// /// <param name="eventCreateDto">Модель создания события</param>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<EventDto> CreateEvent([FromBody] EventCreateDto eventCreateDto)
    {
        var validateResult = ValidateEventDates(eventCreateDto);
        if (validateResult != null)
        {
            return validateResult;
        }
        
        var createdEvent = _eventService.Create(_mapper.Map<Event>(eventCreateDto));

        var uri = Url.Action(nameof(GetEventById), new { id = createdEvent.Id });
        return Created(uri, _mapper.Map<EventDto>(createdEvent));
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="eventDto">Модель события</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventDto> UpdateEvent([FromRoute] Guid id, [FromBody] EventCreateDto eventDto)
    {
        var validateResult = ValidateEventDates(eventDto);
        if (validateResult != null)
        {
            return validateResult;
        }
        
        var eventToUpdate = _mapper.Map<Event>(eventDto);
        eventToUpdate.Id = id;
        
        var createdEvent = _eventService.Update(eventToUpdate);
        if (createdEvent == null)
        {
           return NotFound();
        }
        
        return Ok(_mapper.Map<EventDto>(createdEvent));
    }
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult DeleteEvent(Guid id)
    {
        var isDeleted = _eventService.Delete(id); 
        if (isDeleted) 
        { 
            return NoContent();
        }
        
        // TODO: добавить типизированную ошибку для ситуации, когда сущность не найдена
        // Пока считаем, что может быть только 404 и пробрасываем ее
        return NotFound();
    }


    private ActionResult? ValidateEventDates(EventCreateDto eventCreateDto)
    {
        if (eventCreateDto is { EndAt: not null, StartAt: not null } && eventCreateDto.EndAt < eventCreateDto.StartAt)
        {
            ModelState.AddModelError("EndAt", "Дата окончания события не может быть раньше даты начала");
            
            return ValidationProblem(ModelState);
        }

        return null;
    }
}