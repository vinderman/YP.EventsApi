using AutoMapper;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Services;
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
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = config.CreateMapper();
        _service = new EventService(mapper);
    }
    
    [Fact]
    public void EventService_AddEvent()
    {
        var createEventDto = new EventCreateDto { Title = "Test Event", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow };

        // Act
        var result = _service.Create(createEventDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(result.Id, Guid.Empty);
    }
}