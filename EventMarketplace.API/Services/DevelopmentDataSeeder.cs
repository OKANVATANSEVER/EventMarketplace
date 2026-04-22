using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.API.Services;

public sealed class DevelopmentDataSeeder(ApplicationDbContext dbContext)
{
    private static readonly string[] Cities =
    [
        "Istanbul", "Ankara", "Izmir", "Bursa", "Antalya"
    ];

    private static readonly (string Name, string Description)[] DefaultCategories =
    [
        ("Concert", "Live concerts and music performances"),
        ("Theatre", "Stage plays and theatre shows"),
        ("Festival", "Festivals and large public events")
    ];

    public async Task SeedFakeEventsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await EnsureCategoriesAsync(cancellationToken);

        var existingCount = await dbContext.Events.CountAsync(cancellationToken);
        if (existingCount >= 10)
            return;

        var adminUserId = await dbContext.Users
            .Where(u => u.Email == "admin@eventmarketplace.com")
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(adminUserId))
            return;

        var random = new Random(42);
        var toInsert = 10 - existingCount;

        for (var i = 0; i < toInsert; i++)
        {
            var idx = existingCount + i + 1;
            var start = DateTime.UtcNow.Date.AddDays(idx);
            var end = start.AddHours(4);
            var category = categories[random.Next(categories.Count)];

            dbContext.Events.Add(new Event
            {
                Id = Guid.NewGuid(),
                Title = $"Fake Event {idx}",
                Description = $"Automatically generated fake event #{idx} for development.",
                Price = 50 + (idx * 15),
                StartDate = start,
                EndDate = end,
                City = Cities[random.Next(Cities.Length)],
                CategoryId = category.Id,
                OrganizerId = adminUserId,
                IsFeatured = idx % 3 == 0,
                IsApproved = idx % 2 == 0
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedContentAsync(cancellationToken);
    }

    private async Task SeedContentAsync(CancellationToken cancellationToken)
    {
        if (!await dbContext.SitePageContents.AnyAsync(cancellationToken))
        {
            dbContext.SitePageContents.AddRange(
                new SitePageContent
                {
                    Slug = "about",
                    Title = "Hakkimizda",
                    Body = "EventPazar, sehirdeki etkinlikleri tek bir pazaryerinde bulusturan bir platformdur.",
                    UpdatedAtUtc = DateTime.UtcNow
                },
                new SitePageContent
                {
                    Slug = "contact",
                    Title = "Iletisim",
                    Body = "Bize ulasmak icin hello@eventpazar.com adresine e-posta gonderebilirsiniz.",
                    UpdatedAtUtc = DateTime.UtcNow
                },
                new SitePageContent
                {
                    Slug = "privacy",
                    Title = "Gizlilik",
                    Body = "Kullanici verileri KVKK ve ilgili mevzuat kapsaminda korunur.",
                    UpdatedAtUtc = DateTime.UtcNow
                });
        }

        if (!await dbContext.AdvertisementSlots.AnyAsync(cancellationToken))
        {
            dbContext.AdvertisementSlots.AddRange(
                new AdvertisementSlot
                {
                    Placement = "home-hero",
                    Title = "728x90 Banner Reklami",
                    Subtitle = "Markani ana sayfa hero altinda goster",
                    Priority = 1,
                    IsActive = true
                },
                new AdvertisementSlot
                {
                    Placement = "detail-sidebar",
                    Title = "Detay Sidebar Reklami",
                    Subtitle = "Etkinlik detayinda hedefli gorunum",
                    Priority = 1,
                    IsActive = true
                });
        }

        if (!await dbContext.FeaturedListings.AnyAsync(cancellationToken))
        {
            var events = await dbContext.Events
                .Where(x => x.IsApproved)
                .OrderBy(x => x.StartDate)
                .Take(5)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            for (var i = 0; i < events.Count; i++)
            {
                dbContext.FeaturedListings.Add(new FeaturedListing
                {
                    EventId = events[i],
                    Placement = "home-carousel",
                    Priority = i + 1,
                    IsActive = true,
                    ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<Category>> EnsureCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        if (categories.Count > 0)
            return categories;

        foreach (var (name, description) in DefaultCategories)
        {
            dbContext.Categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
