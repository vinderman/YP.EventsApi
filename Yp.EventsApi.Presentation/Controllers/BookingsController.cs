using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Domain.Enums;
using Yp.EventsApi.Presentation.Contracts;

namespace Yp.EventsApi.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IMapper _mapper;
    
    public BookingsController(IBookingService bookingService, IMapper mapper)
    {
        _bookingService = bookingService;
        _mapper = mapper;
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
        
        return Ok(_mapper.Map<BookingDto>(booking));
    }

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