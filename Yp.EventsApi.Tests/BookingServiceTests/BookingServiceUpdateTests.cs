using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceUpdateTests
{
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    
    public BookingServiceUpdateTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        _mapper = config.CreateMapper();
        _logger = logger.CreateLogger<BookingService>();
    }

    [Fact]
    public async Task BookingService_UpdateBooking_ShouldChangeBooking()
    {
        var bookingId = Guid.NewGuid();

        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService.Setup(e => e.GetById(eventId)).Returns(new EventDto
        {
            Id = eventId,
            Title = "test",
            Description = "test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10,
            AvailableSeats = 10,
        });
        
        var bookingService = new BookingService(_mapper, _logger, eventService.Object);
        
        
        
        var booking = await bookingService.CreateBookingAsync(eventId);

        await bookingService.ConfirmBookingAsync(booking.Id, eventId);
       
       var updatedBooking = await bookingService.GetBookingByIdAsync(booking.Id);
       
       Assert.Equal(booking.Id, updatedBooking.Id);
       Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);

    }
}