using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServicesFindByIdTests
{
    private readonly IEventService _service;
    
    public EventServicesFindByIdTests()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        var mapper = config.CreateMapper();
        _service = new EventService(mapper);
    }
    
    [Fact]
    public void EventService_GetEventByExistingId()
    {
        var existingId = _service.GetAll(new EventFilter()).Items.FirstOrDefault()!.Id;
        var result = _service.GetById(existingId);
        
        Assert.NotNull(result);
        Assert.Equal(existingId, result.Id);
        Assert.IsType<EventDto>(result);
    }
    
    [Fact]
    public void EventService_GetEventByNotExistingId()
    {
        var id = Guid.NewGuid();
        
        Assert.Throws<EntityNotFoundException>(() => _service.GetById(id));
    }
}