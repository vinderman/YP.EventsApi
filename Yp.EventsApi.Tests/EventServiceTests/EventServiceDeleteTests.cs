using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceDeleteTests
{
    private readonly IEventService _service;
    
    public EventServiceDeleteTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _service = new EventService(mapper);
    }
    
    [Fact]
    public void EventService_DeleteEventWithExistingId()
    {
        var allEvents = _service.GetAll(new EventFilter());
        var allEventsCount = allEvents.Total;
        var existingId = allEvents.Items.FirstOrDefault().Id;
        
        _service.Delete(existingId);
        
        var newCount = _service.GetAll(new EventFilter()).Total;
        
        Assert.Equal(allEventsCount - 1, newCount);
    }

    [Fact]
    public void EventService_DeleteEventThrowsWhenEventNotFound()
    {
        var id = Guid.NewGuid();
        Assert.Throws<EntityNotFoundException>(() => _service.Delete(id));
    }
}