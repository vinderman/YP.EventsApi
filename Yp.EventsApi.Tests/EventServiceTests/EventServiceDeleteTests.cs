using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;
using Yp.EventsApi.Services.DataAccess;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Services;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceDeleteTests
{
    private readonly IEventService _service;
    
    private readonly string dbName = Guid.NewGuid().ToString();
    
    public EventServiceDeleteTests()
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
    public async Task EventService_DeleteEventWithExistingId()
    {
        var allEvents = await _service.GetAll(new EventFilter());
        var allEventsCount = allEvents.Total;
        var existingId = allEvents.Items.FirstOrDefault()!.Id;
        
        _service.Delete(existingId);
        
        var newCount = (await _service.GetAll(new EventFilter())).Total;
        
        Assert.Equal(allEventsCount - 1, newCount);
    }

    [Fact]
    public async Task EventService_DeleteEventThrowsWhenEventNotFound()
    {
        var id = Guid.NewGuid();
        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await _service.Delete(id));
    }
}