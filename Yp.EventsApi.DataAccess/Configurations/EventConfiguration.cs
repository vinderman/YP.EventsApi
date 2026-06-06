using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yp.EventsApi.Services.Entities;

namespace Yp.EventsApi.DataAccess.Configurations;

public class EventConfiguration: IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever().IsRequired();
        
        builder.Property(e => e.Title).IsRequired().HasMaxLength(100);
        
        builder.Property(e => e.StartAt).IsRequired();
        
        builder.Property(e => e.EndAt).IsRequired();
        
        builder.Property(e => e.TotalSeats).IsRequired().HasColumnType("smallint");
        
        builder.Property(e => e.AvailableSeats).IsRequired().HasColumnType("smallint");
        
        builder.HasMany(e => e.Bookings).WithOne(e => e.Event).HasForeignKey(e => e.EventId);
    }
}