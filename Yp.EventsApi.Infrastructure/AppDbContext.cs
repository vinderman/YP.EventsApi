using Microsoft.EntityFrameworkCore;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Infrastructure;

public sealed class AppDbContext : DbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}