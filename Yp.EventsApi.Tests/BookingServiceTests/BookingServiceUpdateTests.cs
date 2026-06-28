using Microsoft.Extensions.Logging;
using Moq;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceUpdateTests
{
    private readonly ILogger<BookingService> _logger = Mock.Of<ILogger<BookingService>>();

    [Fact]
    public async Task ConfirmBookingAsync_SetsConfirmedStatusAndProcessedAt()
    {
        var bookingId = Guid.NewGuid();
        var booking = Booking.CreateInstance(bookingId, Guid.NewGuid(), BookingStatus.Pending, Guid.NewGuid());

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new BookingService(
            _logger,
            Mock.Of<IEventService>(),
            bookingRepository.Object,
            unitOfWork.Object);

        await service.ConfirmBookingAsync(bookingId, booking.EventId);

        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public async Task RejectBookingAsync_ReleasesSeatAndSetsRejectedStatus()
    {
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var booking = Booking.CreateInstance(bookingId, eventId, BookingStatus.Pending, Guid.NewGuid());

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var eventService = new Mock<IEventService>();
        eventService
            .Setup(s => s.ReleaseSeats(eventId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new BookingService(
            _logger,
            eventService.Object,
            bookingRepository.Object,
            unitOfWork.Object);

        await service.RejectBookingAsync(bookingId, eventId);

        eventService.Verify(s => s.ReleaseSeats(eventId, 1, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }
}
