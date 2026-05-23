using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.DataAccess;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceGetAllTests
{
    private readonly IEventService _service;
    private readonly string dbName = Guid.NewGuid().ToString();
    public EventServiceGetAllTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName)); 
        var db = services.BuildServiceProvider().GetService<AppDbContext>();
        _service = new EventService(mapper, db);
    }

    [Fact]
    public async Task EventService_GetAllEvents()
    {
        var events = await _service.GetAll(new EventFilter());
        
        Assert.NotNull(events);
        Assert.IsType<PaginatedResult<EventDto>>(events);
        Assert.Equal(5, events.Total);
    }

    [Fact]
    public async Task EventService_FindEventsByTitle()
    {
        var eventsFilter = new EventFilter { Title = "Бокс" };
        
        var events = await _service.GetAll(eventsFilter);
        
        Assert.NotNull(events);
        Assert.Equal(1, events.Total);
    }

    [Fact]
    public async Task EventService_FindEventsByStartAt()
    {
        var startAt = new DateTime(2025, 03, 12);
        var eventsFilter = new EventFilter { From = startAt };
        
        var events = await _service.GetAll(eventsFilter);

        Assert.NotNull(events);
        Assert.Equal(3, events.Total);
        Assert.All(events.Items, item => Assert.True(item.StartAt >= startAt));
    }
    
    [Fact]
    public async Task EventService_FindEventsByEndAt()
    {
        var endAt = new DateTime(2025, 03, 12);
        var eventsFilter = new EventFilter { To = endAt };
        
        var events = await _service.GetAll(eventsFilter);

        Assert.NotNull(events);
        Assert.Equal(1, events.Total);
        Assert.All(events.Items, item => Assert.True(item.EndAt <= endAt));
    }

    [Fact]
    public async Task EventService_PaginateEventsWithCustomPageSize()
    {
        var eventsFilter = new EventFilter { PageSize = 1, Page = 1};
        var events = await _service.GetAll(eventsFilter);
        
        Assert.NotNull(events);
        Assert.Single(events.Items);
    }

    [Fact]
    public async Task EventService_PaginateEventsWithCustomPage()
    {
        var eventsFilter = new EventFilter { Page = 2, PageSize = 2};
        var events = await _service.GetAll(eventsFilter);
        
        Assert.NotNull(events);
        Assert.Equal(2, events.Items.Count());
        Assert.Equal(2, events.CurrentPage);
    }
}