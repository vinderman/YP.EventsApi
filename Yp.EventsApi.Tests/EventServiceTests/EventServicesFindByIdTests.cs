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

public class EventServicesFindByIdTests
{
    private readonly IEventService _service;
    
    private readonly string dbName = Guid.NewGuid().ToString();
    public EventServicesFindByIdTests()
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
    public async Task EventService_GetEventByExistingId()
    {
        var existingId = (await _service.GetAll(new EventFilter())).Items.FirstOrDefault()!.Id;
        var result = await _service.GetById(existingId);
        
        Assert.NotNull(result);
        Assert.Equal(existingId, result.Id);
        Assert.IsType<EventDto>(result);
    }
    
    [Fact]
    public async Task EventService_GetEventByNotExistingId()
    {
        var id = Guid.NewGuid();
        
        await Assert.ThrowsAsync<EntityNotFoundException>(async() => await _service.GetById(id));
    }
}