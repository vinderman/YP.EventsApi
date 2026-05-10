using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.DataAccess;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceUpdateTests
{
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    private readonly string dbName = Guid.NewGuid().ToString();
    private readonly AppDbContext _dbContext;
    
    public BookingServiceUpdateTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        _mapper = config.CreateMapper();
        _logger = logger.CreateLogger<BookingService>();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        _dbContext = services.BuildServiceProvider().GetRequiredService<AppDbContext>();
    }

    [Fact]
    public async Task BookingService_ConfirmBooking_ShouldSetConfirmedStatusAndProcessedAt()
    {
        // Arrange
        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService.Setup(e => e.TryReserveSeats(eventId, 1)).ReturnsAsync(true);
        
        var bookingService = new BookingService(_mapper, _logger, eventService.Object, _dbContext);

        var booking = await bookingService.CreateBookingAsync(eventId);

        // Act
        await bookingService.ConfirmBookingAsync(booking.Id, eventId);
       
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
        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService.Setup(e => e.TryReserveSeats(eventId, 1)).ReturnsAsync(true);
        eventService.Setup(e => e.ReleaseSeats(eventId, 1)).ReturnsAsync(true);

        var bookingService = new BookingService(_mapper, _logger, eventService.Object, _dbContext);
        var booking = await bookingService.CreateBookingAsync(eventId);

        // Act
        await bookingService.RejectBookingAsync(booking.Id, eventId);
        var updatedBooking = await bookingService.GetBookingByIdAsync(booking.Id);

        // Assert
        Assert.Equal(BookingStatus.Rejected, updatedBooking.Status);
        Assert.NotNull(updatedBooking.ProcessedAt);
    }

    [Fact]
    public async Task BookingService_RejectBooking_ShouldReleaseSeats()
    {
        // Arrange
        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService.Setup(e => e.TryReserveSeats(eventId, 1)).ReturnsAsync(true);
        eventService.Setup(e => e.ReleaseSeats(eventId, 1)).ReturnsAsync(true);

        var bookingService = new BookingService(_mapper, _logger, eventService.Object, _dbContext);
        var booking = await bookingService.CreateBookingAsync(eventId);

        // Act
        await bookingService.RejectBookingAsync(booking.Id, eventId);

        // Assert
        eventService.Verify(e => e.ReleaseSeats(eventId, 1), Times.Once);
    }

    [Fact]
    public async Task BookingService_RejectBooking_WhenSeatsReleased_ShouldAllowCreatingNewBookingForSameSeat()
    {
        // Arrange
        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService
            .SetupSequence(e => e.TryReserveSeats(eventId, 1))
            .ReturnsAsync(true)
            .ThrowsAsync(new NoAvailableSeatsException("Для данного события нет доступных мест"))
            .ReturnsAsync(true);
        eventService.Setup(e => e.ReleaseSeats(eventId, 1)).ReturnsAsync(true);

        var bookingService = new BookingService(_mapper, _logger, eventService.Object, _dbContext);
        var booking = await bookingService.CreateBookingAsync(eventId);
        await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await bookingService.CreateBookingAsync(eventId));

        // Act
        await bookingService.RejectBookingAsync(booking.Id, eventId);
        var newBooking = await bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotNull(newBooking);
        Assert.Equal(BookingStatus.Pending, newBooking.Status);
    }
}