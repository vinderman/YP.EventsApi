using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yp.EventsApi.Application.Models;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Presentation.Contracts;

namespace Yp.EventsApi.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController: ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IValidator<EventCreateDto> _eventCreateDtoValidator;
    private readonly IBookingService _bookingService;
    private readonly IMapper _mapper;
    
    public EventsController(IEventService eventService, IValidator<EventCreateDto> eventCreateDtoValidator, IBookingService bookingService, IMapper mapper)
    {
        _eventService = eventService;
        _eventCreateDtoValidator = eventCreateDtoValidator;
        _bookingService = bookingService;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить все события
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<EventDto>>> GetEvents([FromQuery] EventFilter filter, CancellationToken cancellationToken)
    {
        var result = await _eventService.GetAll(filter, cancellationToken);
        
        return Ok(new PaginatedResult<EventDto>
        {
            Total = result.Total,
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            Items = _mapper.Map<IEnumerable<EventDto>>(result.Items)
        });
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id, CancellationToken cancellationToken)
    {
        var eventById = await _eventService.GetById(id, cancellationToken);
        
        return Ok(_mapper.Map<EventDto>(eventById));
    }
    
    /// <summary>
    /// Создать событие
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] EventCreateDto eventCreateDto, CancellationToken cancellationToken)
    {
        var validateResult = _eventCreateDtoValidator.Validate(eventCreateDto);
        if (!validateResult.IsValid)
        {
            throw new ValidationException("Произошла ошибка", validateResult.Errors);
        }
        
        var createdEvent = await _eventService.Create(_mapper.Map<CreateEventRequest>(eventCreateDto), cancellationToken);
        var uri = Url.Action(nameof(GetEventById), new { id = createdEvent.Id });
        
        return Created(uri, _mapper.Map<EventDto>(createdEvent));
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UpdateEvent([FromRoute] Guid id, [FromBody] EventCreateDto eventCreateDto, CancellationToken cancellationToken)
    {
        var validateResult = _eventCreateDtoValidator.Validate(eventCreateDto);
        if (!validateResult.IsValid)
        {
            throw new ValidationException("Произошла ошибка", validateResult.Errors);
        }
        
        var createdEvent = await _eventService.Update(id, _mapper.Map<UpdateEventRequest>(eventCreateDto), cancellationToken);
        
        return Ok(_mapper.Map<EventDto>(createdEvent));
    }
    
    /// <summary>
    /// Удалить событие
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken)
    {
        await _eventService.Delete(id, cancellationToken); 
        
        return NoContent();
    }

    /// <summary>
    /// Создание бронирования, связанного с определенным событием
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("{id}/book")]
    [ProducesResponseType(typeof(ActionResult<BookingDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> BookEvent([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.Identity.Name);
        var booking = await _bookingService.CreateBookingAsync(id, userId, ct);
        return AcceptedAtAction(actionName: nameof(BookingsController.GetBookingById), controllerName: "Bookings", routeValues: new { id = booking.Id }, value: _mapper.Map<BookingDto>(booking));
    }
}