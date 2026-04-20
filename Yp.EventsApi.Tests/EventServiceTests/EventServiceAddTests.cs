using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;

namespace Yp.EventsApi.Tests.EventServiceTests;

/// <summary>
/// Класс для тестов на функционал добавление событий
/// </summary>
public class EventServiceAddTests
{
    private readonly IEventService _service;
    
    public EventServiceAddTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _service = new EventService(mapper);
    }
    
    [Fact]
    public void EventService_AddEvent()
    {
        var startAt = DateTime.UtcNow;
        var createEventDto = new EventCreateDto
        {
            Title = "Test Event",
            StartAt = startAt,
            EndAt = startAt.AddHours(1),
            TotalSeats = 10
        };

        // Act
        var result = _service.Create(createEventDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(result.Id, Guid.Empty);
    }
}