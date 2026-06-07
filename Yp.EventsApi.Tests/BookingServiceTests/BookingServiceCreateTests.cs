
using Microsoft.Extensions.Logging;
using Moq;
using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceCreateTests
{
    private readonly ILogger<BookingService> _logger = Mock.Of<ILogger<BookingService>>();

    [Fact]
    public async Task CreateBookingAsync_ReservesSeatPersistsBookingAndReturnsPendingStatus()
    {
        var eventId = Guid.NewGuid();
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.TryReserveSeats(eventId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new BookingService(_logger, eventService.Object, bookingRepository.Object, unitOfWork.Object);

        var result = await service.CreateBookingAsync(eventId);

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
        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.TryReserveSeats(eventId, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NoAvailableSeatsException("Для данного события нет доступных мест"));

        var service = new BookingService(
            _logger,
            eventService.Object,
            Mock.Of<IBookingRepository>(),
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => service.CreateBookingAsync(eventId));
    }
}
