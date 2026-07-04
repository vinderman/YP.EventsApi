using Microsoft.EntityFrameworkCore;
using Yp.EventsApi.Infrastructure;

namespace Yp.EventsApi.IntegrationTests.Infrastructure;

internal static class DatabaseCleaner
{
    public static async Task CleanAsync(AppDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE "Bookings", "Events", "Users" CASCADE;
            """);
    }
}
