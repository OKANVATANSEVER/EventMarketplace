using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMarketplace.Infrastructure.Persistence.Configurations;

public sealed class UserCategoryPreferenceConfiguration : IEntityTypeConfiguration<UserCategoryPreference>
{
    public void Configure(EntityTypeBuilder<UserCategoryPreference> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.WantsEmail)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.WantsSms)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.CategoryId }).IsUnique();
    }
}
