using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace YP.EventApi.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController: ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IValidator<EventCreateDto> _eventCreateDtoValidator;
    private readonly IBookingService _bookingService;
    
    public EventsController(IEventService eventService, IValidator<EventCreateDto> eventCreateDtoValidator, IBookingService bookingService)
    {
        _eventService = eventService;
        _eventCreateDtoValidator = eventCreateDtoValidator;
        _bookingService = bookingService;
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<EventDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<EventDto>> GetEvents([FromQuery] EventFilter filter)
    {
        var events = _eventService.GetAll(filter);
        
        return Ok(events);
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id)
    {
        var eventById = await _eventService.GetById(id);
        return Ok(eventById);
    }
    
    /// <summary>
    /// Создать событие
    /// </summary>
    /// /// <param name="eventCreateDto">Модель создания события</param>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] EventCreateDto eventCreateDto)
    {
        var validateResult = _eventCreateDtoValidator.Validate(eventCreateDto);
        if (!validateResult.IsValid)
        {
            throw new ValidationException("Произошла ошибка", validateResult.Errors);
        }
        
        var createdEvent = await _eventService.Create(eventCreateDto);
        var uri = Url.Action(nameof(GetEventById), new { id = createdEvent.Id });
        
        return Created(uri, createdEvent);
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    /// <param name="id"></param>
    /// <param name="eventCreateDto">Модель события</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UpdateEvent([FromRoute] Guid id, [FromBody] EventCreateDto eventCreateDto)
    {
        var validateResult = _eventCreateDtoValidator.Validate(eventCreateDto);
        if (!validateResult.IsValid)
        {
            throw new ValidationException("Произошла ошибка", validateResult.Errors);
        }
        
        var createdEvent = await _eventService.Update(id, eventCreateDto);
        
        return Ok(createdEvent);
    }
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        await _eventService.Delete(id); 
        
        return NoContent();
    }

    /// <summary>
    /// Создание бронирования, связанного с определенным событием
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("{id}/book")]
    [ProducesResponseType(typeof(ActionResult<BookingDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> BookEvent([FromRoute] Guid id, CancellationToken ct)
    {
        var booking = await _bookingService.CreateBookingAsync(id, ct);
        return AcceptedAtAction(actionName: nameof(BookingsController.GetBookingById), controllerName: "Bookings", routeValues: new { id = booking.Id }, value: booking);
    }
}