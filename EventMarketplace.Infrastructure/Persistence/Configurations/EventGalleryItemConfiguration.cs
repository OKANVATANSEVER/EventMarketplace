using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class EventGalleryItemConfiguration : IEntityTypeConfiguration<EventGalleryItem>
{
    public void Configure(EntityTypeBuilder<EventGalleryItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(600)
            .IsRequired();

        builder.Property(x => x.Caption)
            .HasMaxLength(260);

        builder.HasOne(x => x.Event)
            .WithMany(e => e.GalleryItems)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EventId, x.SortOrder });
    }
}
