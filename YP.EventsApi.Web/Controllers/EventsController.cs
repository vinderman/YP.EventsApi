using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Yp.EventsApi.Services.Dto;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Services;

namespace YP.EventApi.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public ActionResult<EventDto> GetEventById(int id)
    {
        var eventById = _eventService.GetById(id);
        
        return Ok(_mapper.Map<EventDto>(eventById));
    }
    
    /// <summary>
    /// Создать событие
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<EventDto> CreateEvent([FromBody] EventCreateDto eventCreateDto)
    {
        if (eventCreateDto.EndAt < eventCreateDto.StartAt)
        {
            ModelState.AddModelError("EndAt", "Дата окончания события не может быть позднее даты начала");
            
            return BadRequest(ModelState);
        }
        
        
        var createdEvent = _eventService.Create(_mapper.Map<Event>(eventCreateDto));
        
        return Ok(_mapper.Map<EventDto>(createdEvent));
    }


    [HttpPut("{id}")]
    public ActionResult<EventDto> UpdateEvent(int id, [FromBody] EventDto eventDto)
    {
        var newEvent = _eventService.Update(_mapper.Map<Event>(eventDto));
        
        return Ok(_mapper.Map<EventDto>(newEvent));
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteEvent(int id)
    {
        try
        {
            _eventService.Delete(id);
            return NoContent();
        }
        catch (Exception e)
        {
            // TODO: добавить типизированную ошибку для ситуации, когда сущность не найдена
            // Пока считаем, что может быть только 404 и пробрасываем ее
            return NotFound("Событие не найдено");
        }
    }
}