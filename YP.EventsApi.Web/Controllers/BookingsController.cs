using Microsoft.AspNetCore.Mvc;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Shared.Contracts;

namespace YP.EventApi.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ActionResult<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetBookingById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id, cancellationToken);
        
        return Ok(booking);
    }
}