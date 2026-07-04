using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;
using Yp.EventsApi.Infrastructure;
using Yp.EventsApi.Infrastructure.Repositories;
using Yp.EventsApi.IntegrationTests.Infrastructure;

namespace Yp.EventsApi.IntegrationTests.Repositories;

[Collection(nameof(PostgresCollection))]
public class BookingRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public BookingRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateAsync_AndGetByIdAsync_PersistBooking()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        var eventEntity = TestDataSeed.SeedEvent(context);
        var user = TestDataSeed.SeedUser(context);

        var repository = new BookingRepository(context);
        var booking = Booking.CreateInstance(Guid.NewGuid(), eventEntity.Id, BookingStatus.Pending, user.Id);
        var unitOfWork = new EfUnitOfWork(context);

        await repository.CreateAsync(booking);
        await unitOfWork.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(booking.Id);

        Assert.NotNull(loaded);
        Assert.Equal(booking.Id, loaded.Id);
        Assert.Equal(BookingStatus.Pending, loaded.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);

        var repository = new BookingRepository(context);
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByStatusAsync_ReturnsOnlyMatchingBookings()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        var eventEntity = TestDataSeed.SeedEvent(context);
        var user = TestDataSeed.SeedUser(context);

        var repository = new BookingRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        var pending = Booking.CreateInstance(Guid.NewGuid(), eventEntity.Id, BookingStatus.Pending, user.Id);
        var confirmed = Booking.CreateInstance(Guid.NewGuid(), eventEntity.Id, BookingStatus.Confirmed, user.Id);
        confirmed.Status = BookingStatus.Confirmed;

        await repository.CreateAsync(pending);
        await repository.CreateAsync(confirmed);
        await unitOfWork.SaveChangesAsync();

        var pendingBookings = await repository.GetAllByStatusAsync(BookingStatus.Pending);

        Assert.Single(pendingBookings);
        Assert.Equal(pending.Id, pendingBookings[0].Id);
    }
}
