using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class FeaturedListingConfiguration : IEntityTypeConfiguration<FeaturedListing>
{
    public void Configure(EntityTypeBuilder<FeaturedListing> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Placement)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(x => x.Event)
            .WithMany(e => e.FeaturedListings)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Placement, x.Priority });
        builder.HasIndex(x => x.EventId);
    }
}
