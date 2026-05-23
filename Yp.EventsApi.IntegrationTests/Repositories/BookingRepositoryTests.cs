using Yp.EventsApi.DataAccess.Repositories;
using Yp.EventsApi.IntegrationTests.Infrastructure;
using Yp.EventsApi.Services.Entities;
using Yp.EventsApi.Shared.Enums;

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

        var repository = new BookingRepository(context);
        var booking = Booking.CreateInstance(Guid.NewGuid(), eventEntity.Id, BookingStatus.Pending);

        await repository.CreateAsync(booking);
        await context.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(booking.Id);

        Assert.NotNull(loaded);
        Assert.Equal(booking.Id, loaded.Id);
        Assert.Equal(BookingStatus.Pending, loaded.Status);
    }

    [Fact]
    public async Task GetAllByStatusAsync_ReturnsOnlyMatchingBookings()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        var eventEntity = TestDataSeed.SeedEvent(context);

        var repository = new BookingRepository(context);
        var pending = Booking.CreateInstance(Guid.NewGuid(), eventEntity.Id, BookingStatus.Pending);
        var confirmed = Booking.CreateInstance(Guid.NewGuid(), eventEntity.Id, BookingStatus.Confirmed);
        confirmed.Status = BookingStatus.Confirmed;

        await repository.CreateAsync(pending);
        await repository.CreateAsync(confirmed);
        await context.SaveChangesAsync();

        var pendingBookings = await repository.GetAllByStatusAsync(BookingStatus.Pending);

        Assert.Single(pendingBookings);
        Assert.Equal(pending.Id, pendingBookings[0].Id);
    }
}
