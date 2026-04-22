using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class EventCommentConfiguration : IEntityTypeConfiguration<EventComment>
{
    public void Configure(EntityTypeBuilder<EventComment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasMaxLength(1200)
            .IsRequired();

        builder.Property(x => x.IsApproved)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(x => x.Event)
            .WithMany(e => e.Comments)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
