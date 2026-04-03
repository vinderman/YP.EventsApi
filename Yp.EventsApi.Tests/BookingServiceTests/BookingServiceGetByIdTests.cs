using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Shared.Contracts;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceGetByIdTests
{
    private readonly IBookingService _service;
    public BookingServiceGetByIdTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _service = new BookingService(mapper, logger.CreateLogger<BookingService>());
    }

    [Fact]
    public async Task BookingService_GetById_ShouldReturnBooking()
    {
        var eventId = Guid.NewGuid();
        var createdBooking = await _service.CreateBookingAsync(eventId);
        
        var booking = await _service.GetBookingByIdAsync(createdBooking.Id);
        
        Assert.NotNull(booking);
        Assert.IsType<BookingDto>(booking);
    }
    
    [Fact]
    public async Task BookingService_GetById_GetByNotExistIdThrows()
    {
        var randomId = Guid.NewGuid();

        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await _service.GetBookingByIdAsync(randomId));
    }
}