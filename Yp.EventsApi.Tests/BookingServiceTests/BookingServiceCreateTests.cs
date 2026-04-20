using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceCreateTests
{
    private readonly ILogger<BookingService> _logger;
    private readonly IMapper _mapper;
    public BookingServiceCreateTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _mapper = mapper;
        _logger = logger.CreateLogger<BookingService>();
    }

    [Fact]
    public async Task BookingService_AddBooking_ShouldReturnPendingBooking()
    {
        // Arrange
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
        
        // Act
        var newBooking = await bookingService.CreateBookingAsync(eventId);
        
        // Assert
        Assert.NotNull(newBooking);
        Assert.Equal(BookingStatus.Pending, newBooking.Status);
    }

    [Fact]
    public async Task BookingService_AddBooking_ShouldCreateUniqueId()
    {
        // Arrange
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
        
        
        // Act
        var booking1 = await bookingService.CreateBookingAsync(eventId);
        var booking2 = await bookingService.CreateBookingAsync(eventId);
        
        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
    }

    [Fact]
    public async Task BookingService_AddBooking_ShouldThrowExceptionIfEventIdIsInvalid()
    {
        var eventService = new EventService(_mapper);
        var eventId = Guid.NewGuid();
        
        var bookingService = new BookingService(_mapper, _logger, eventService);
        
        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await bookingService.CreateBookingAsync(eventId));
    }
}