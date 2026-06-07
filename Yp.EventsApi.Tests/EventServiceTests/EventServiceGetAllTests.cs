using Moq;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Models;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceGetAllTests
{

    [Fact]
    public async Task GetAll_MapsRepositoryResultToPaginatedDto()
    {
        var events = new List<Event>
        {
            Event.CreateInstance(Guid.NewGuid(), "A", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 1),
            Event.CreateInstance(Guid.NewGuid(), "B", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 1)
        };
        var filter = new EventFilter { Page = 2, PageSize = 1 };

        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetPagedAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync((events.Take(1).ToList(), 2));

        var service = new EventService(eventRepository.Object, Mock.Of<IUnitOfWork>());
        var result = await service.GetAll(filter, CancellationToken.None);

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(1, result.PageSize);
        Assert.Single(result.Items);
    }
}
