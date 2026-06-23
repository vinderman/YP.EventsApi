using Yp.EventsApi.Application.Exceptions;
using Yp.EventsApi.Application.Services.EventService;
using Yp.EventsApi.Infrastructure;
using Yp.EventsApi.Infrastructure.Repositories;
using Yp.EventsApi.IntegrationTests.Infrastructure;

namespace Yp.EventsApi.IntegrationTests.Services;

[Collection(nameof(PostgresCollection))]
public class EventServiceReserveSeatsTests
{
    private readonly PostgresFixture _fixture;

    public EventServiceReserveSeatsTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task TryReserveSeats_ConcurrentRequests_ShouldPreventOverbooking()
    {
        await using var setupContext = _fixture.CreateContext();
        await DatabaseCleaner.CleanAsync(setupContext);
        var eventEntity = TestDataSeed.SeedEvent(setupContext, totalSeats: 5);
        var eventId = eventEntity.Id;

        var tasks = Enumerable.Range(0, 20).Select(async _ =>
        {
            await using var context = _fixture.CreateContext();
            var service = CreateEventService(context);

            try
            {
                await service.TryReserveSeats(eventId, 1, CancellationToken.None);
                return (Success: true, Error: (Exception?)null);
            }
            catch (Exception ex)
            {
                return (Success: false, Error: ex);
            }
        });

        var results = await Task.WhenAll(tasks);
        var successful = results.Count(r => r.Success);
        var failed = results.Where(r => !r.Success).Select(r => r.Error).ToList();

        Assert.Equal(5, successful);
        Assert.Equal(15, failed.Count);
        Assert.All(failed, ex => Assert.IsType<NoAvailableSeatsException>(ex));

        await using var verifyContext = _fixture.CreateContext();
        var verifyService = CreateEventService(verifyContext);
        var updated = await verifyService.GetById(eventId, CancellationToken.None);
        Assert.Equal(0, updated.AvailableSeats);
    }

    private static IEventService CreateEventService(AppDbContext context)
    {
        var eventRepository = new EventRepository(context);
        var unitOfWork = new EfUnitOfWork(context);
        return new EventService(eventRepository, unitOfWork);
    }
}
