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
    public async Task BookingService_CreateBooking_ShouldReturnPendingBooking()
    {
        // Arrange
        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService.Setup(e => e.TryReserveSeats(eventId, 1)).Returns(true);
        
        var bookingService = new BookingService(_mapper, _logger, eventService.Object);
        
        // Act
        var newBooking = await bookingService.CreateBookingAsync(eventId);
        
        // Assert
        Assert.NotNull(newBooking);
        Assert.Equal(BookingStatus.Pending, newBooking.Status);
    }

    [Fact]
    public async Task BookingService_CreateBooking_MultipleBookingsWithinLimit_ShouldSucceedAndHaveUniqueIds()
    {
        // Arrange
        var eventService = new EventService(_mapper);
        var createdEvent = eventService.Create(new EventCreateDto
        {
            Title = "test",
            Description = "test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 3,
        });

        var bookingService = new BookingService(_mapper, _logger, eventService);

        // Act
        var booking1 = await bookingService.CreateBookingAsync(createdEvent.Id);
        var booking2 = await bookingService.CreateBookingAsync(createdEvent.Id);
        var booking3 = await bookingService.CreateBookingAsync(createdEvent.Id);
        
        // Assert
        Assert.NotNull(booking1);
        Assert.NotNull(booking2);
        Assert.NotNull(booking3);
        Assert.Equal(3, new HashSet<Guid> { booking1.Id, booking2.Id, booking3.Id }.Count);
    }

    [Fact]
    public async Task BookingService_CreateBooking_ShouldDecreaseAvailableSeatsByOne()
    {
        // Arrange
        var eventService = new EventService(_mapper);
        var createdEvent = eventService.Create(new EventCreateDto
        {
            Title = "test",
            Description = "test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 2,
        });

        var before = eventService.GetById(createdEvent.Id);
        
        var bookingService = new BookingService(_mapper, _logger, eventService);

        // Act
        await bookingService.CreateBookingAsync(createdEvent.Id);
        var after = eventService.GetById(createdEvent.Id);
        
        // Assert
        Assert.Equal(before.AvailableSeats - 1, after.AvailableSeats);
    }

    [Fact]
    public async Task BookingService_CreateBooking_AfterSeatsExhausted_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        var eventService = new EventService(_mapper);
        var createdEvent = eventService.Create(new EventCreateDto
        {
            Title = "test",
            Description = "test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 1,
        });

        var bookingService = new BookingService(_mapper, _logger, eventService);

        // Act
        _ = await bookingService.CreateBookingAsync(createdEvent.Id);

        // Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await bookingService.CreateBookingAsync(createdEvent.Id));
    }

    [Fact]
    public async Task BookingService_CreateBooking_ForNonExistingEvent_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var eventService = new EventService(_mapper);
        var bookingService = new BookingService(_mapper, _logger, eventService);

        // Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await bookingService.CreateBookingAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task BookingService_CreateBooking_WhenNoSeatsAvailable_ShouldThrowNoAvailableSeatsException()
    {
        // Arrange
        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService
            .Setup(e => e.TryReserveSeats(eventId, 1))
            .Throws(new NoAvailableSeatsException("Для данного события нет доступных мест"));

        var bookingService = new BookingService(_mapper, _logger, eventService.Object);

        // Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await bookingService.CreateBookingAsync(eventId));
    }
}