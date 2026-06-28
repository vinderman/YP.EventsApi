
using Microsoft.Extensions.Logging;
using Moq;
using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;
using Yp.EventsApi.Domain.Exceptions;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceCreateTests
{
    private readonly ILogger<BookingService> _logger = Mock.Of<ILogger<BookingService>>();

    [Fact]
    public async Task CreateBookingAsync_ReservesSeatPersistsBookingAndReturnsPendingStatus()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var futureEvent = CreateFutureEvent(eventId);

        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetById(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureEvent);
        eventService
            .Setup(s => s.TryReserveSeats(eventId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.CountActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new BookingService(_logger, eventService.Object, bookingRepository.Object, unitOfWork.Object);

        var result = await service.CreateBookingAsync(eventId, userId);

        eventService.Verify(s => s.TryReserveSeats(eventId, 1, It.IsAny<CancellationToken>()), Times.Once);
        bookingRepository.Verify(r => r.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Equal(eventId, result.EventId);
    }

    [Fact]
    public async Task CreateBookingAsync_PropagatesNoAvailableSeatsException()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var futureEvent = CreateFutureEvent(eventId);

        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetById(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureEvent);
        eventService
            .Setup(s => s.TryReserveSeats(eventId, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NoAvailableSeatsException("Для данного события нет доступных мест"));

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.CountActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new BookingService(
            _logger,
            eventService.Object,
            bookingRepository.Object,
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => service.CreateBookingAsync(eventId, userId));
    }

    [Fact]
    public async Task CreateBookingAsync_ThrowsBookingEventException_WhenEventAlreadyStarted()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pastEvent = Event.CreateInstance(
            eventId,
            "Past event",
            DateTime.Now.AddHours(-2),
            DateTime.Now.AddHours(-1),
            10);

        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetById(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastEvent);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.CountActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new BookingService(
            _logger,
            eventService.Object,
            bookingRepository.Object,
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<BookingEventException>(
            () => service.CreateBookingAsync(eventId, userId));

        eventService.Verify(s => s.TryReserveSeats(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        bookingRepository.Verify(r => r.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookingAsync_ThrowsBookingCountExceededException_WhenActiveLimitReached()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var eventService = new Mock<IEventService>();
        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.CountActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var service = new BookingService(
            _logger,
            eventService.Object,
            bookingRepository.Object,
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<BookingCountExceededException>(
            () => service.CreateBookingAsync(eventId, userId));

        eventService.Verify(s => s.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        eventService.Verify(s => s.TryReserveSeats(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        bookingRepository.Verify(r => r.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookingAsync_DoesNotApplyOtherUsersActiveBookingLimit()
    {
        var eventId = Guid.NewGuid();
        var userAtLimit = Guid.NewGuid();
        var userBelowLimit = Guid.NewGuid();
        var futureEvent = CreateFutureEvent(eventId);

        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.GetById(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureEvent);
        eventService
            .Setup(s => s.TryReserveSeats(eventId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.CountActiveByUserIdAsync(userAtLimit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        bookingRepository
            .Setup(r => r.CountActiveByUserIdAsync(userBelowLimit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new BookingService(_logger, eventService.Object, bookingRepository.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<BookingCountExceededException>(
            () => service.CreateBookingAsync(eventId, userAtLimit));

        var result = await service.CreateBookingAsync(eventId, userBelowLimit);

        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Equal(userBelowLimit, result.UserId);
        bookingRepository.Verify(
            r => r.CreateAsync(It.Is<Booking>(b => b.UserId == userBelowLimit), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Event CreateFutureEvent(Guid eventId) =>
        Event.CreateInstance(
            eventId,
            "Future event",
            DateTime.Now.AddHours(1),
            DateTime.Now.AddHours(2),
            10);
}
