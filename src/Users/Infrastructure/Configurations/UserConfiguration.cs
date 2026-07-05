using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever().IsRequired();
        
        builder.Property(u => u.Login).IsRequired().HasMaxLength(100);
        
        builder.HasIndex(u => u.Login).IsUnique();
        
        builder.Property(u => u.Role).IsRequired().HasConversion<string>();
        
        builder.Property(u => u.PasswordHash).IsRequired();
    }
}