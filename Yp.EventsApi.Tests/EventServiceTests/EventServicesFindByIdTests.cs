using AutoMapper;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServicesFindByIdTests
{
    private readonly IEventService _service;
    
    public EventServicesFindByIdTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = config.CreateMapper();
        _service = new EventService(mapper);
    }
    
    [Fact]
    public void EventService_GetEventByExistingId()
    {
        var existingId = _service.GetAll(new EventFilter()).Items.FirstOrDefault().Id;
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