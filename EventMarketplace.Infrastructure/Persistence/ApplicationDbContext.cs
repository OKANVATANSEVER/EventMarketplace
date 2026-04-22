using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Identity;
using EventMarketplace.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<UserApp, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SitePageContent> SitePageContents => Set<SitePageContent>();
    public DbSet<AdvertisementSlot> AdvertisementSlots => Set<AdvertisementSlot>();
    public DbSet<FeaturedListing> FeaturedListings => Set<FeaturedListing>();
    public DbSet<EventComment> EventComments => Set<EventComment>();
    public DbSet<EventGalleryItem> EventGalleryItems => Set<EventGalleryItem>();
    public DbSet<EventMessage> EventMessages => Set<EventMessage>();
    public DbSet<UserCategoryPreference> UserCategoryPreferences => Set<UserCategoryPreference>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        IdentitySeed.Seed(builder);
        CategorySeed.Seed(builder);
    }
}