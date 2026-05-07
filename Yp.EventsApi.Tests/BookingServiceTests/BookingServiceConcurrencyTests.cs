using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services.BookingService;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;

namespace Yp.EventsApi.Tests.BookingServiceTests;

public class BookingServiceConcurrencyTests
{
    private readonly ILogger<BookingService> _logger;
    private readonly IMapper _mapper;

    public BookingServiceConcurrencyTests()
    {
        var loggerFactory = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory);
        _mapper = config.CreateMapper();
        _logger = loggerFactory.CreateLogger<BookingService>();
    }

    [Fact]
    public async Task BookingService_CreateBooking_ConcurrentRequests_ShouldPreventOverbooking()
    {
        // Arrange
        var eventService = new EventService(_mapper);
        var createdEvent = eventService.Create(new EventCreateDto
        {
            Title = "test",
            Description = "test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 5,
        });

        var bookingService = new BookingService(_mapper, _logger, eventService);

        // Act
        var tasks = Enumerable.Range(0, 20).Select(async _ =>
        {
            try
            {
                var booking = await bookingService.CreateBookingAsync(createdEvent.Id);
                return (booking.Id, null);
            }
            catch (Exception ex)
            {
                return (BookingId: (Guid?)null, Error: ex);
            }
        }).ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        var successful = results.Where(r => r.Error is null).ToList();
        var failed = results.Where(r => r.Error is not null).ToList();

        Assert.Equal(5, successful.Count);
        Assert.Equal(15, failed.Count);
        Assert.All(failed, r => Assert.IsType<NoAvailableSeatsException>(r.Error));

        var updatedEvent = eventService.GetById(createdEvent.Id);
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task BookingService_CreateBooking_ConcurrentRequests_ShouldCreateUniqueIds()
    {
        // Arrange
        var eventService = new EventService(_mapper);
        var createdEvent = eventService.Create(new EventCreateDto
        {
            Title = "test",
            Description = "test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10,
        });

        var bookingService = new BookingService(_mapper, _logger, eventService);

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => bookingService.CreateBookingAsync(createdEvent.Id))
            .ToArray();

        var bookings = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, bookings.Length);
        Assert.Equal(10, bookings.Select(b => b.Id).Distinct().Count());

        var updatedEvent = eventService.GetById(createdEvent.Id);
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }
}

