using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.Property(x => x.OrganizerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.IsFeatured)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsApproved)
            .HasDefaultValue(false)
            .IsRequired();

        // Relationship: Event belongs to Category
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Event belongs to Organizer (UserApp) — no navigation on Event side
        builder.HasOne<UserApp>()
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.OrganizerId);
        builder.HasIndex(x => x.IsFeatured);
        builder.HasIndex(x => x.IsApproved);
        builder.HasIndex(x => x.StartDate);
    }
}