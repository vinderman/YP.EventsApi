using Yp.EventsApi.Domain.Entities;
using Yp.EventsApi.Domain.Enums;
using Yp.EventsApi.Infrastructure;

namespace Yp.EventsApi.IntegrationTests.Infrastructure;

internal static class TestDataSeed
{
    public static void SeedEvents(AppDbContext dbContext)
    {
        dbContext.Events.AddRange(
            Event.CreateInstance(Guid.NewGuid(), "Тренировка по боксу", Utc(2025, 3, 20), Utc(2025, 4, 20), 10),
            Event.CreateInstance(Guid.NewGuid(), "День рождения", Utc(2024, 3, 20), Utc(2026, 3, 20), 10),
            Event.CreateInstance(Guid.NewGuid(), "Корпоратив", Utc(2023, 3, 20), Utc(2024, 3, 20), 10),
            Event.CreateInstance(Guid.NewGuid(), "Поездка на море", Utc(2026, 4, 20), Utc(2026, 5, 13), 10),
            Event.CreateInstance(Guid.NewGuid(), "Свадьба", Utc(2026, 6, 10), Utc(2026, 6, 10), 10));

        dbContext.SaveChanges();
    }
    
    
    public static User SeedUser(AppDbContext dbContext, int totalSeats = 10)
    {
        var entity = User.CreateInstance(
            Guid.NewGuid(),
            "test",
            "sadsadas",
            UserRole.Admin);

        dbContext.Users.Add(entity);
        dbContext.SaveChanges();
        return entity;
    }

    public static Event SeedEvent(AppDbContext dbContext, int totalSeats = 10)
    {
        var entity = Event.CreateInstance(
            Guid.NewGuid(),
            "test",
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            totalSeats);

        dbContext.Events.Add(entity);
        dbContext.SaveChanges();
        return entity;
    }

    private static DateTime Utc(int year, int month, int day)
        => DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Utc);
}
