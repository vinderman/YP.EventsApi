using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
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
    public async Task BookingService_ConfirmBooking_ShouldSetConfirmedStatusAndProcessedAt()
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

        var booking = await bookingService.CreateBookingAsync(createdEvent.Id);

        // Act
        await bookingService.ConfirmBookingAsync(booking.Id, createdEvent.Id);
       
        var updatedBooking = await bookingService.GetBookingByIdAsync(booking.Id);
       
        // Assert
        Assert.Equal(booking.Id, updatedBooking.Id);
        Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);
        Assert.NotNull(updatedBooking.ProcessedAt);

    }

    [Fact]
    public async Task BookingService_RejectBooking_ShouldSetRejectedStatusAndProcessedAt()
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
        var booking = await bookingService.CreateBookingAsync(createdEvent.Id);

        // Act
        await bookingService.RejectBookingAsync(booking.Id, createdEvent.Id);
        var updatedBooking = await bookingService.GetBookingByIdAsync(booking.Id);

        // Assert
        Assert.Equal(BookingStatus.Rejected, updatedBooking.Status);
        Assert.NotNull(updatedBooking.ProcessedAt);
    }

    [Fact]
    public async Task BookingService_RejectBooking_ShouldReleaseSeatsAndRestoreAvailableSeats()
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
        var booking = await bookingService.CreateBookingAsync(createdEvent.Id);
        var afterCreate = eventService.GetById(createdEvent.Id);
        Assert.Equal(0, afterCreate.AvailableSeats);

        // Act
        await bookingService.RejectBookingAsync(booking.Id, createdEvent.Id);
        var afterReject = eventService.GetById(createdEvent.Id);

        // Assert
        Assert.Equal(1, afterReject.AvailableSeats);
    }

    [Fact]
    public async Task BookingService_RejectBooking_WhenSeatsReleased_ShouldAllowCreatingNewBookingForSameSeat()
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
        var booking = await bookingService.CreateBookingAsync(createdEvent.Id);
        await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await bookingService.CreateBookingAsync(createdEvent.Id));

        // Act
        await bookingService.RejectBookingAsync(booking.Id, createdEvent.Id);
        var newBooking = await bookingService.CreateBookingAsync(createdEvent.Id);

        // Assert
        Assert.NotNull(newBooking);
        Assert.Equal(BookingStatus.Pending, newBooking.Status);
    }
}