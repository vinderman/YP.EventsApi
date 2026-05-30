using Yp.EventsApi.DataAccess;
using Yp.EventsApi.DataAccess.Repositories;
using Yp.EventsApi.IntegrationTests.Infrastructure;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Shared.Models;

namespace Yp.EventsApi.IntegrationTests.Repositories;

[Collection(nameof(PostgresCollection))]
public class EventRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public EventRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetPagedAsync_ReturnsAllSeededEvents()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        TestDataSeed.SeedEvents(context);

        var repository = new EventRepository(context);
        var (items, total) = await repository.GetPagedAsync(new EventFilter());

        Assert.Equal(5, total);
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByTitle()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        TestDataSeed.SeedEvents(context);

        var repository = new EventRepository(context);
        var (items, total) = await repository.GetPagedAsync(new EventFilter { Title = "Бокс" });

        Assert.Equal(1, total);
        Assert.Contains("боксу", items.Single().Title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByStartAt()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        TestDataSeed.SeedEvents(context);

        var startAt = DateTime.SpecifyKind(new DateTime(2025, 3, 12), DateTimeKind.Utc);
        var repository = new EventRepository(context);
        var (items, total) = await repository.GetPagedAsync(new EventFilter { From = startAt });

        Assert.Equal(3, total);
        Assert.All(items, item => Assert.True(item.StartAt >= startAt));
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByEndAt()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        TestDataSeed.SeedEvents(context);

        var endAt = DateTime.SpecifyKind(new DateTime(2025, 3, 12), DateTimeKind.Utc);
        var repository = new EventRepository(context);
        var (items, total) = await repository.GetPagedAsync(new EventFilter { To = endAt });

        Assert.Equal(1, total);
        Assert.All(items, item => Assert.True(item.EndAt <= endAt));
    }

    [Fact]
    public async Task GetPagedAsync_PaginatesResults()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        TestDataSeed.SeedEvents(context);

        var repository = new EventRepository(context);

        var firstPage = await repository.GetPagedAsync(new EventFilter { PageSize = 1, Page = 1 });
        var secondPage = await repository.GetPagedAsync(new EventFilter { Page = 2, PageSize = 2 });

        Assert.Single(firstPage.Items);
        Assert.Equal(5, firstPage.Total);
        Assert.Equal(2, secondPage.Items.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEvent_WhenExists()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        var seeded = TestDataSeed.SeedEvent(context);

        var repository = new EventRepository(context);
        var result = await repository.GetByIdAsync(seeded.Id);

        Assert.NotNull(result);
        Assert.Equal(seeded.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);

        var repository = new EventRepository(context);
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ReturnsEvent_WhenExists()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        var seeded = TestDataSeed.SeedEvent(context);

        var repository = new EventRepository(context);
        await using var transaction = await context.Database.BeginTransactionAsync();
        var result = await repository.GetByIdForUpdateAsync(seeded.Id);
        await transaction.CommitAsync();

        Assert.NotNull(result);
        Assert.Equal(seeded.Id, result.Id);
        Assert.Equal(seeded.AvailableSeats, result.AvailableSeats);
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ReturnsNull_WhenNotExists()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);

        var repository = new EventRepository(context);
        await using var transaction = await context.Database.BeginTransactionAsync();
        var result = await repository.GetByIdForUpdateAsync(Guid.NewGuid());
        await transaction.CommitAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AndRemove_PersistChanges()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);

        var repository = new EventRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var entity = Event.CreateInstance(
            Guid.NewGuid(),
            "Новое событие",
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            5);

        await repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();

        Assert.NotNull(await repository.GetByIdAsync(entity.Id));

        repository.Remove(entity.Id);
        await unitOfWork.SaveChangesAsync();

        Assert.Null(await repository.GetByIdAsync(entity.Id));
    }
}
