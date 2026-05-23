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

public class EventServiceUpdateTests
{
    private readonly IEventService _service;
    
    private readonly string dbName = Guid.NewGuid().ToString();
    public EventServiceUpdateTests()
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
    public async Task EventService_UpdateEvent()
    {
        var existingId = (await _service.GetAll(new EventFilter())).Items.FirstOrDefault()!.Id;
        var createEventDto = new EventCreateDto { Title = "Test Event", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow };
        
        var updatedEvent = await _service.Update(existingId, createEventDto);
        
        Assert.NotNull(updatedEvent);
        Assert.Equal(existingId, updatedEvent.Id);
        Assert.Equal(createEventDto.Title, updatedEvent.Title);
        Assert.Equal(createEventDto.StartAt, updatedEvent.StartAt);
        Assert.Equal(createEventDto.EndAt, updatedEvent.EndAt);
    }
    
    [Fact]
    public async Task EventService_UpdateEventWithNotExistingId()
    {
        var id = Guid.NewGuid();
        var createEventDto = new EventCreateDto { Title = "Test Event", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow };
        
        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await _service.Update(id, createEventDto));
    }
}