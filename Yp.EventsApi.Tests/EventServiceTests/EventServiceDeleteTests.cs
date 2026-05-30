using Moq;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Tests.Common;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceDeleteTests
{
    private readonly IMapper _mapper = ServiceTestFactory.CreateMapper();

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
        var service = new EventService(_mapper, eventRepository.Object, unitOfWork.Object);

        await service.Delete(eventId, CancellationToken.None);

        eventRepository.Verify(r => r.Remove(eventId), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ThrowsEntityNotFoundException_WhenEventMissing()
    {
        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var service = new EventService(_mapper, eventRepository.Object, Mock.Of<IUnitOfWork>());

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => service.Delete(Guid.NewGuid(), CancellationToken.None));
    }
}
