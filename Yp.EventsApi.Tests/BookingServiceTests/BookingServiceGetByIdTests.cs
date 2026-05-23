using Microsoft.Extensions.Logging;
using Moq;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Enums;
using Yp.EventsApi.Tests.Common;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceGetByIdTests
{
    private readonly AutoMapper.IMapper _mapper = ServiceTestFactory.CreateMapper();
    private readonly ILogger<BookingService> _logger = Mock.Of<ILogger<BookingService>>();

    [Fact]
    public async Task GetBookingByIdAsync_ReturnsMappedDto_WhenBookingExists()
    {
        var bookingId = Guid.NewGuid();
        var booking = Booking.CreateInstance(bookingId, Guid.NewGuid(), BookingStatus.Pending);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var service = new BookingService(
            _mapper,
            _logger,
            Mock.Of<IEventService>(),
            bookingRepository.Object,
            Mock.Of<IUnitOfWork>());

        var result = await service.GetBookingByIdAsync(bookingId);

        Assert.IsType<BookingDto>(result);
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
            _mapper,
            _logger,
            Mock.Of<IEventService>(),
            bookingRepository.Object,
            Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => service.GetBookingByIdAsync(Guid.NewGuid()));
    }
}
