using AutoMapper;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Shared.Contracts;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceTests
{
    private readonly IEventService _service;
    public EventServiceTests()
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

    [Fact]
    public void EventService_GetAllEvents()
    {
        var events = _service.GetAll(new EventFilter());
        
        Assert.NotNull(events);
        Assert.IsType<PaginatedResult<EventDto>>(events);
        Assert.Equal(5, events.Total);
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

    [Fact]
    public void EventService_DeleteEvent()
    {
        var allEvents = _service.GetAll(new EventFilter());
        var allEventsCount = allEvents.Total;
        var existingId = allEvents.Items.FirstOrDefault().Id;
        
        _service.Delete(existingId);
        
        var newCount = _service.GetAll(new EventFilter()).Total;
        
        Assert.Equal(allEventsCount - 1, newCount);
        Assert.Throws<EntityNotFoundException>(() => _service.GetById(existingId));
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