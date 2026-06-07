using Moq;
using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServicesFindByIdTests
{

    [Fact]
    public async Task GetById_ReturnsMappedDto_WhenEventExists()
    {
        var eventId = Guid.NewGuid();
        var entity = Event.CreateInstance(eventId, "Test", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 5);

        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var service = new EventService(eventRepository.Object, Mock.Of<IUnitOfWork>());
        var result = await service.GetById(eventId, CancellationToken.None);

        Assert.Equal(eventId, result.Id);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetById_ThrowsEntityNotFoundException_WhenEventMissing()
    {
        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var service = new EventService(eventRepository.Object, Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => service.GetById(Guid.NewGuid(), CancellationToken.None));
    }
}
