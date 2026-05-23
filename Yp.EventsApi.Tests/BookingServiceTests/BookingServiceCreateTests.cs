using Microsoft.Extensions.Logging;
using Moq;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Enums;
using Yp.EventsApi.Tests.Common;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceCreateTests
{
    private readonly AutoMapper.IMapper _mapper = ServiceTestFactory.CreateMapper();
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
        var service = new BookingService(_mapper, _logger, eventService.Object, bookingRepository.Object, unitOfWork.Object);

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
            _mapper,
            _logger,
            eventService.Object,
            Mock.Of<IBookingRepository>(),
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => service.CreateBookingAsync(eventId));
    }
}
