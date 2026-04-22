using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class EventMessageConfiguration : IEntityTypeConfiguration<EventMessage>
{
    public void Configure(EntityTypeBuilder<EventMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SenderEmail)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(x => x.MessageText)
            .HasMaxLength(1500)
            .IsRequired();

        builder.HasOne(x => x.Event)
            .WithMany(e => e.Messages)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
