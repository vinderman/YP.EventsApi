using Moq;
using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Interfaces;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceDeleteTests
{
    

    [Fact]
    public async Task Delete_RemovesEventAndSavesChanges()
    {
        var eventId = Guid.NewGuid();
        var entity = Event.CreateInstance(eventId, "Test", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 3);

        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new EventService(eventRepository.Object, unitOfWork.Object);

        await service.Delete(eventId, CancellationToken.None);

        eventRepository.Verify(r => r.Remove(entity), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ThrowsEntityNotFoundException_WhenEventMissing()
    {
        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var service = new EventService(eventRepository.Object, Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => service.Delete(Guid.NewGuid(), CancellationToken.None));
    }
}
