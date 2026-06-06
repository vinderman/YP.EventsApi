using Moq;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Services.Exceptions;
using Yp.EventsApi.Services.Interfaces;
using Yp.EventsApi.Services.Services.EventService;
using Yp.EventsApi.Tests.Common;

namespace Yp.EventsApi.Tests.EventServiceTests;

public class EventServiceReserveSeatsTests
{
    private readonly IMapper _mapper = ServiceTestFactory.CreateMapper();

    [Fact]
    public async Task TryReserveSeats_CommitsTransaction_WhenSeatAvailable()
    {
        var eventId = Guid.NewGuid();
        var entity = Event.CreateInstance(eventId, "Test", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 2);

        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdForUpdateAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var transaction = new Mock<IUnitOfWorkTransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);

        var service = new EventService(_mapper, eventRepository.Object, unitOfWork.Object);

        var result = await service.TryReserveSeats(eventId, 1, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(1, entity.AvailableSeats);
        transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TryReserveSeats_RollsBack_WhenNoSeatsAvailable()
    {
        var eventId = Guid.NewGuid();
        var entity = Event.CreateInstance(eventId, "Test", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 1);
        entity.TryReserveSeats(1);

        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdForUpdateAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var transaction = new Mock<IUnitOfWorkTransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);

        var service = new EventService(_mapper, eventRepository.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => service.TryReserveSeats(eventId, 1, CancellationToken.None));

        transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TryReserveSeats_RollsBack_WhenEventNotFound()
    {
        var eventRepository = new Mock<IEventRepository>();
        eventRepository
            .Setup(r => r.GetByIdForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var transaction = new Mock<IUnitOfWorkTransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);

        var service = new EventService(_mapper, eventRepository.Object, unitOfWork.Object);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => service.TryReserveSeats(Guid.NewGuid(), 1, CancellationToken.None));

        transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReleaseSeats_ReturnsFalse_WhenSeatsCountIsNotPositive()
    {
        var service = new EventService(_mapper, Mock.Of<IEventRepository>(), Mock.Of<IUnitOfWork>());

        var result = await service.ReleaseSeats(Guid.NewGuid(), 0, CancellationToken.None);

        Assert.False(result);
    }
}
