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
using Yp.EventsApi.Shared.Contracts;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceGetByIdTests
{
    private readonly ILogger<BookingService> _logger;
    private readonly IMapper _mapper;
    private readonly string dbName = Guid.NewGuid().ToString();
    private readonly AppDbContext _dbContext;

    public BookingServiceGetByIdTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _mapper = mapper;
        _logger = logger.CreateLogger<BookingService>();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        _dbContext = services.BuildServiceProvider().GetRequiredService<AppDbContext>();
    }

    [Fact]
    public async Task BookingService_GetById_ShouldReturnBooking()
    {
        var eventService = new Mock<IEventService>();
        var eventId = Guid.NewGuid();
        eventService.Setup(e => e.TryReserveSeats(eventId, 1)).ReturnsAsync(true);
        eventService.Setup(e => e.GetById(eventId)).ReturnsAsync(new EventDto
        {
            Id = eventId,
            Title = "test",
            Description = "test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10,
            AvailableSeats = 10,
        });

        var bookingService = new BookingService(_mapper, _logger, eventService.Object, _dbContext);

        var createdBooking = await bookingService.CreateBookingAsync(eventId);

        var booking = await bookingService.GetBookingByIdAsync(createdBooking.Id);

        Assert.NotNull(booking);
        Assert.IsType<BookingDto>(booking);
    }

    [Fact]
    public async Task BookingService_GetById_GetByNotExistIdThrows()
    {
        var eventService = new Mock<IEventService>();
        var bookingService = new BookingService(_mapper, _logger, eventService.Object, _dbContext);
        var randomId = Guid.NewGuid();

        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await bookingService.GetBookingByIdAsync(randomId));
    }
}
