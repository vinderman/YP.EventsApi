using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services.BookingService;
using Presentation.Contracts;
using Shared.Domain.Enums;

namespace Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Создание бронирования для события
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ActionResult<BookingDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> CreateBooking([FromQuery] Guid eventId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(HttpContext.User.Identity.Name);
        var booking = await _bookingService.CreateBookingAsync(eventId, userId, cancellationToken);

        return AcceptedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking.ToDto());
    }

    /// <summary>
    /// Получение бронирование по идентификатору
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ActionResult<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> GetBookingById(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(HttpContext.User.Identity.Name);
        var role = Enum.Parse<UserRole>(User.FindFirstValue("role")!);
        var booking = await _bookingService.GetBookingByIdAsync(id, userId, role, cancellationToken);
        
        return Ok(booking.ToDto());
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookingDto>> CancelBookingById(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(HttpContext.User.Identity.Name);
        var role = Enum.Parse<UserRole>(User.FindFirstValue("role")!);

        await _bookingService.CancelBookingAsync(id, userId, role, cancellationToken);
        
        return NoContent();
    }
}
