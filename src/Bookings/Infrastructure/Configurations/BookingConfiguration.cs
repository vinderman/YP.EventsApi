using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BookingConfiguration: IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever().IsRequired();
        
        builder.Property(e => e.EventId).IsRequired();
        
        builder.Property(e => e.Status).IsRequired().HasConversion<string>();
        
        builder.Property(e => e.CreatedAt).IsRequired();
        
        builder.Property(e => e.UserId).ValueGeneratedNever().IsRequired();
    }
}
