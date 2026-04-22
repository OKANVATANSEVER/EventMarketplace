using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class AdvertisementSlotConfiguration : IEntityTypeConfiguration<AdvertisementSlot>
{
    public void Configure(EntityTypeBuilder<AdvertisementSlot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Placement)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.Subtitle)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(x => x.LinkUrl)
            .HasMaxLength(600);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(600);

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(450);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.Placement);
        builder.HasIndex(x => new { x.Placement, x.Priority });
    }
}
