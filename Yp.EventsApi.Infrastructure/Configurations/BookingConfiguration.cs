using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yp.EventsApi.Domain.Entities;

namespace Yp.EventsApi.Infrastructure.Configurations;

public class BookingConfiguration: IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever().IsRequired();
        
        builder.HasOne(b => b.Event).WithMany(e => e.Bookings).HasForeignKey(e => e.EventId);
        
        builder.Property(e => e.Status).IsRequired().HasConversion<string>();
        
        builder.Property(e => e.CreatedAt).IsRequired();
        
        builder.Property(e => e.UserId).ValueGeneratedNever().IsRequired();
    }
}