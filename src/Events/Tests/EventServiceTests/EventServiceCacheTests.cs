using Application;
using Application.Interfaces;
using Application.Models;
using Application.Services.EventService;
using Domain.Entities;
using Moq;
using Shared.UnitOfWork;

namespace Tests.EventServiceTests;

public class EventServiceCacheTests
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly Mock<IEventRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICacheService> _cache = new();

    private EventService Sut => new(_repository.Object, _unitOfWork.Object, _cache.Object);

    private static Event CreateEvent(Guid? id = null, int seats = 10) =>
        Event.CreateInstance(
            id ?? Guid.NewGuid(),
            "Тестовое событие",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            seats);

    private void ArrangeTransaction()
    {
        var tx = new Mock<IUnitOfWorkTransaction>();
        tx.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

        _unitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tx.Object);
    }

    private void VerifyCacheSet(Guid eventId, Event value) =>
        _cache.Verify(c => c.Set(CacheKeys.EventById(eventId), value, CacheTtl), Times.Once);

    [Fact(DisplayName = "GetById: при попадании в кеш репозиторий не вызывается")]
    public async Task GetById_CacheHit()
    {
        var id = Guid.NewGuid();
        var cached = CreateEvent(id);

        _cache.Setup(c => c.GetByKey<Event>(CacheKeys.EventById(id))).ReturnsAsync(cached);

        var result = await Sut.GetById(id);

        Assert.Same(cached, result);
        _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _cache.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<Event>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact(DisplayName = "GetById: при промахе данные берутся из репозитория и сохраняются в кеш")]
    public async Task GetById_CacheMiss()
    {
        var id = Guid.NewGuid();
        var fromDb = CreateEvent(id);

        _cache.Setup(c => c.GetByKey<Event>(CacheKeys.EventById(id))).ReturnsAsync((Event?)null);
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(fromDb);

        var result = await Sut.GetById(id);

        Assert.Same(fromDb, result);
        VerifyCacheSet(id, fromDb);
    }

    [Fact(DisplayName = "GetTopSelledEvents: при попадании в кеш репозиторий не вызывается")]
    public async Task GetTopSelledEvents_CacheHit()
    {
        IReadOnlyList<Event> cached = [CreateEvent(), CreateEvent()];

        _cache.Setup(c => c.GetByKey<IReadOnlyList<Event>>(CacheKeys.TopSoldEvents)).ReturnsAsync(cached);

        var result = await Sut.GetTopSelledEvents(10, default);

        Assert.Same(cached, result);
        _repository.Verify(r => r.GetTopSelledEvents(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GetTopSelledEvents: при промахе данные берутся из репозитория и сохраняются в кеш")]
    public async Task GetTopSelledEvents_CacheMiss()
    {
        IReadOnlyList<Event> fromDb = [CreateEvent()];

        _cache.Setup(c => c.GetByKey<IReadOnlyList<Event>>(CacheKeys.TopSoldEvents)).ReturnsAsync((IReadOnlyList<Event>?)null);
        _repository.Setup(r => r.GetTopSelledEvents(5, It.IsAny<CancellationToken>())).ReturnsAsync(fromDb);

        var result = await Sut.GetTopSelledEvents(5, default);

        Assert.Same(fromDb, result);
        _cache.Verify(c => c.Set(CacheKeys.TopSoldEvents, fromDb, CacheTtl), Times.Once);
    }

    [Fact(DisplayName = "Update: после изменения события кеш обновляется (write-through)")]
    public async Task Update_WriteThrough()
    {
        var id = Guid.NewGuid();
        var existing = CreateEvent(id);
        var request = new UpdateEventRequest
        {
            Title = "Новое название",
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(2).AddHours(3),
            TotalSeats = 20
        };

        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await Sut.Update(id, request, default);

        VerifyCacheSet(id, existing);
        _cache.Verify(c => c.Delete(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "TryReserveSeats: после резерва мест кеш обновляется (write-through)")]
    public async Task TryReserveSeats_WriteThrough()
    {
        var id = Guid.NewGuid();
        var entity = CreateEvent(id, seats: 5);
        ArrangeTransaction();

        _repository.Setup(r => r.GetByIdForUpdateAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await Sut.TryReserveSeats(id, 1, default);

        Assert.Equal(4, entity.AvailableSeats);
        VerifyCacheSet(id, entity);
    }

    [Fact(DisplayName = "ReleaseSeats: после освобождения мест кеш обновляется (write-through)")]
    public async Task ReleaseSeats_WriteThrough()
    {
        var id = Guid.NewGuid();
        var entity = CreateEvent(id, seats: 5);
        entity.TryReserveSeats(2);
        ArrangeTransaction();

        _repository.Setup(r => r.GetByIdForUpdateAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await Sut.ReleaseSeats(id, 1, default);

        Assert.Equal(4, entity.AvailableSeats);
        VerifyCacheSet(id, entity);
    }

    [Fact(DisplayName = "Delete: при удалении события запись в кеше инвалидируется")]
    public async Task Delete_Invalidate()
    {
        var id = Guid.NewGuid();
        var entity = CreateEvent(id);

        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await Sut.Delete(id, default);

        _cache.Verify(c => c.Delete(CacheKeys.EventById(id)), Times.Once);
        _cache.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<Event>(), It.IsAny<TimeSpan?>()), Times.Never);
    }
}
