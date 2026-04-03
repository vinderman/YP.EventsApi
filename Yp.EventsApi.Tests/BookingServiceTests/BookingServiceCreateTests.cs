using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Shared.Enums;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceCreateTests
{
    private readonly IBookingService _service;
    public BookingServiceCreateTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _service = new BookingService(mapper, logger.CreateLogger<BookingService>());
    }

    [Fact]
    public async Task BookingService_AddBooking_ShouldReturnPendingBooking()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        
        
        // Act
        var newBooking = await _service.CreateBookingAsync(eventId);
        
        // Assert
        Assert.NotNull(newBooking);
        Assert.Equal(BookingStatus.Pending, newBooking.Status);
    }

    [Fact]
    public async Task BookingService_AddBooking_ShouldCreateUniqueId()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        
        
        // Act
        var booking1 = await _service.CreateBookingAsync(eventId);
        var booking2 = await _service.CreateBookingAsync(eventId);
        
        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
    }
}