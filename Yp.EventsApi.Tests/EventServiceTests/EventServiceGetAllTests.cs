using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceGetAllTests
{
    private readonly IEventService _service;
    public EventServiceGetAllTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _service = new EventService(mapper);
    }

    [Fact]
    public void EventService_GetAllEvents()
    {
        var events = _service.GetAll(new EventFilter());
        
        Assert.NotNull(events);
        Assert.IsType<PaginatedResult<EventDto>>(events);
        Assert.Equal(5, events.Total);
    }

    [Fact]
    public void EventService_FindEventsByTitle()
    {
        var eventsFilter = new EventFilter { Title = "Бокс" };
        
        var events = _service.GetAll(eventsFilter);
        
        Assert.NotNull(events);
        Assert.Equal(1, events.Total);
    }

    [Fact]
    public void EventService_FindEventsByStartAt()
    {
        var startAt = new DateTime(2025, 03, 12);
        var eventsFilter = new EventFilter { From = startAt };
        
        var events = _service.GetAll(eventsFilter);

        Assert.NotNull(events);
        Assert.Equal(3, events.Total);
        Assert.All(events.Items, item => Assert.True(item.StartAt >= startAt));
    }
    
    [Fact]
    public void EventService_FindEventsByEndAt()
    {
        var endAt = new DateTime(2025, 03, 12);
        var eventsFilter = new EventFilter { To = endAt };
        
        var events = _service.GetAll(eventsFilter);

        Assert.NotNull(events);
        Assert.Equal(1, events.Total);
        Assert.All(events.Items, item => Assert.True(item.EndAt <= endAt));
    }

    [Fact]
    public void EventService_PaginateEventsWithCustomPageSize()
    {
        var eventsFilter = new EventFilter { PageSize = 1, Page = 1};
        var events = _service.GetAll(eventsFilter);
        
        Assert.NotNull(events);
        Assert.Single(events.Items);
    }

    [Fact]
    public void EventService_PaginateEventsWithCustomPage()
    {
        var eventsFilter = new EventFilter { Page = 2, PageSize = 2};
        var events = _service.GetAll(eventsFilter);
        
        Assert.NotNull(events);
        Assert.Equal(2, events.Items.Count());
        Assert.Equal(2, events.CurrentPage);
    }
}