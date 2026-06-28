using Microsoft.Extensions.Logging;
using Moq;
using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.BookingService;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceGetByIdTests
{
    private readonly ILogger<BookingService> _logger = Mock.Of<ILogger<BookingService>>();

    [Fact]
    public async Task GetBookingByIdAsync_ReturnsMappedDto_WhenBookingExists()
    {
        var bookingId = Guid.NewGuid();
        var booking = Booking.CreateInstance(bookingId, Guid.NewGuid(), BookingStatus.Pending, Guid.NewGuid());

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var service = new BookingService(
            _logger,
            Mock.Of<IEventService>(),
            bookingRepository.Object,
            Mock.Of<IUnitOfWork>());

        var result = await service.GetBookingByIdAsync(bookingId);

        Assert.IsType<Booking>(result);
        Assert.Equal(bookingId, result.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ThrowsEntityNotFoundException_WhenBookingMissing()
    {
        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var service = new BookingService(
            _logger,
            Mock.Of<IEventService>(),
            bookingRepository.Object,
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => service.GetBookingByIdAsync(Guid.NewGuid()));
    }
}
