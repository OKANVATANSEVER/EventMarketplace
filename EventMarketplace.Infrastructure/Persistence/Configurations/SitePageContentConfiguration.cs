using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class SitePageContentConfiguration : IEntityTypeConfiguration<SitePageContent>
{
    public void Configure(EntityTypeBuilder<SitePageContent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
