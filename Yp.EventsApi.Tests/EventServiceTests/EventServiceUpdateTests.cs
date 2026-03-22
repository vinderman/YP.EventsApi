using AutoMapper;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceUpdateTests
{
    private readonly IEventService _service;
    
    public EventServiceUpdateTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = config.CreateMapper();
        _service = new EventService(mapper);
    }
    
    [Fact]
    public void EventService_UpdateEvent()
    {
        var existingId = _service.GetAll(new EventFilter()).Items.FirstOrDefault().Id;
        var createEventDto = new EventCreateDto { Title = "Test Event", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow };
        
        var updatedEvent = _service.Update(existingId, createEventDto);
        
        Assert.NotNull(updatedEvent);
        Assert.Equal(existingId, updatedEvent.Id);
        Assert.Equal(createEventDto.Title, updatedEvent.Title);
        Assert.Equal(createEventDto.StartAt, updatedEvent.StartAt);
        Assert.Equal(createEventDto.EndAt, updatedEvent.EndAt);
    }
    
    [Fact]
    public void EventService_UpdateEventWithNotExistingId()
    {
        var id = Guid.NewGuid();
        var createEventDto = new EventCreateDto { Title = "Test Event", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow };
        
        Assert.Throws<EntityNotFoundException>(() => _service.Update(id, createEventDto));
    }
}