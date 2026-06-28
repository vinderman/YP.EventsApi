using Microsoft.EntityFrameworkCore;
using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;
using Yp.EventsApi.IntegrationTests.Infrastructure;

namespace Yp.EventsApi.IntegrationTests.Migrations;

[Collection(nameof(PostgresCollection))]
public class DatabaseSchemaTests
{
    private const string InitialMigrationId = "20260523102812_InitialCreate";

    private readonly PostgresFixture _fixture;

    public DatabaseSchemaTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Migrate_CreatesExpectedSchema()
    {
        await using var context = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(context);
        await context.Database.MigrateAsync();

        Assert.Contains(InitialMigrationId, await context.Database.GetAppliedMigrationsAsync());
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());

        Assert.False(await context.Events.AnyAsync());
        Assert.False(await context.Bookings.AnyAsync());

        context.Bookings.Add(Booking.CreateInstance(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending, Guid.NewGuid()));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
