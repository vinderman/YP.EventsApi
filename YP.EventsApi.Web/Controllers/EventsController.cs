using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
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
    private readonly IValidator<EventCreateDto> _eventCreateDtoValidator;
    
    public EventsController(IEventService eventService, IMapper mapper, IValidator<EventCreateDto> eventCreateDtoValidator)
    {
        _eventService = eventService;
        _mapper = mapper;
        _eventCreateDtoValidator = eventCreateDtoValidator;
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
        var validateResult = _eventCreateDtoValidator.Validate(eventCreateDto);
        if (!validateResult.IsValid)
        {
            throw new ValidationException("Произошла ошибка", validateResult.Errors);
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
        var validateResult = _eventCreateDtoValidator.Validate(eventDto);
        if (!validateResult.IsValid)
        {
            throw new ValidationException("Произошла ошибка", validateResult.Errors);
        }
        
        var eventToUpdate = _mapper.Map<Event>(eventDto);
        eventToUpdate.Id = id;
        
        var createdEvent = _eventService.Update(eventToUpdate);
        
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
        _eventService.Delete(id); 
        
        return NoContent();
    }
}