using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.API.Services;

public sealed class DevelopmentDataSeeder(ApplicationDbContext dbContext)
{
    private static readonly string[] Cities =
    [
        "Istanbul", "Ankara", "Izmir", "Bursa", "Antalya",
        "Adana", "Gaziantep", "Konya", "Eskisehir", "Trabzon"
    ];

    private static readonly string[] Venues =
    [
        "Zorlu PSM", "Volkswagen Arena", "IF Performance Hall",
        "Jolly Joker", "Babylon", "KüçükÇiftlik Park",
        "Harbiye Açık Hava", "Congresium", "Bostancı Gösteri Merkezi",
        "Moda Sahnesi"
    ];

    private static readonly string[] EventTitles =
    [
        "Rock Festivali", "Jazz Gecesi", "Klasik Müzik Konseri",
        "Stand-up Gösterisi", "DJ Party", "Tiyatro Oyunu",
        "Dans Gösterisi", "Edebiyat Söyleşisi", "Açık Hava Sineması",
        "Fotoğraf Sergisi", "Yemek Festivali", "Teknoloji Zirvesi",
        "Yoga & Meditasyon", "Çocuk Tiyatrosu", "Sanat Atölyesi",
        "Müzik Yarışması", "Komedi Gecesi", "Opera Performansı",
        "Sokak Sanatları", "Elektronik Müzik Festivali",
        "Indie Rock Konseri", "Blues Akşamı", "Flamenco Gecesi",
        "Pop Konseri", "Hip-Hop Sahne", "Country Müzik Gecesi",
        "Reggae Festivali", "Metal Konseri", "Akustik Performans",
        "Caz Festivali"
    ];

    private static readonly string[] CommentContents =
    [
        "Harika bir etkinlikti, kesinlikle tekrar katılırım!",
        "Organizasyon çok iyiydi, tebrikler.",
        "Mekan biraz küçüktü ama performans harikaydı.",
        "Bilet fiyatı biraz yüksek ama değdi.",
        "Arkadaşlarımla çok eğlendik, tavsiye ederim.",
        "Ses sistemi mükemmeldi!",
        "Beklentilerimin üzerindeydi.",
        "Çocuklarla gittik, herkes çok memnun kaldı.",
        "Bir sonraki etkinliği sabırsızlıkla bekliyorum.",
        "Atmosfer çok güzeldi, herkese öneririm."
    ];

    private static readonly string[] DisplayNames =
    [
        "Ahmet Y.", "Ayşe K.", "Mehmet B.", "Fatma T.", "Ali D.",
        "Zeynep S.", "Mustafa Ö.", "Elif A.", "Hasan C.", "Selin M.",
        "Can E.", "Deniz R.", "Emre P.", "Gizem U.", "Burak L."
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
        if (existingCount >= 30)
            return;

        var adminUserId = await dbContext.Users
            .Where(u => u.Email == "admin@eventmarketplace.com")
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(adminUserId))
            return;

        var random = new Random(42);
        var toInsert = 30 - existingCount;

        for (var i = 0; i < toInsert; i++)
        {
            var idx = existingCount + i + 1;
            var start = DateTime.UtcNow.Date.AddDays(random.Next(1, 90));
            var end = start.AddHours(random.Next(2, 8));
            var category = categories[random.Next(categories.Count)];
            var price = Math.Round((decimal)(random.NextDouble() * 450 + 50), 2);

            dbContext.Events.Add(new Event
            {
                Id = Guid.NewGuid(),
                Title = EventTitles[i % EventTitles.Length],
                Description = $"{EventTitles[i % EventTitles.Length]} - {Cities[random.Next(Cities.Length)]} şehrinde unutulmaz bir deneyim sizi bekliyor. Biletinizi şimdi alın!",
                Price = price,
                StartDate = start,
                EndDate = end,
                City = Cities[random.Next(Cities.Length)],
                CategoryId = category.Id,
                OrganizerId = adminUserId,
                IsFeatured = idx % 3 == 0,
                IsApproved = idx % 5 != 0
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedContentAsync(cancellationToken);
        await SeedPopularVenuesAsync(cancellationToken);
        await SeedCommentsAsync(cancellationToken);
    }

    private async Task SeedCommentsAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.EventComments.AnyAsync(cancellationToken))
            return;

        var approvedEvents = await dbContext.Events
            .Where(e => e.IsApproved)
            .Select(e => e.Id)
            .Take(15)
            .ToListAsync(cancellationToken);

        var random = new Random(123);

        foreach (var eventId in approvedEvents)
        {
            var commentCount = random.Next(2, 5);
            for (var i = 0; i < commentCount; i++)
            {
                dbContext.EventComments.Add(new EventComment
                {
                    EventId = eventId,
                    DisplayName = DisplayNames[random.Next(DisplayNames.Length)],
                    Content = CommentContents[random.Next(CommentContents.Length)],
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    IsApproved = random.NextDouble() > 0.15
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPopularVenuesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.PopularVenues.AnyAsync(cancellationToken))
            return;

        var venueData = new[]
        {
            ("Zorlu PSM", "Istanbul", "konser"),
            ("Volkswagen Arena", "Istanbul", "arena"),
            ("IF Performance Hall", "Ankara", "performans"),
            ("Jolly Joker", "Istanbul", "bar"),
            ("Babylon", "Istanbul", "kulüp"),
            ("KüçükÇiftlik Park", "Istanbul", "açık-hava"),
            ("Harbiye Açık Hava", "Istanbul", "açık-hava"),
            ("Congresium", "Ankara", "kongre"),
            ("Bostancı Gösteri Merkezi", "Istanbul", "gösteri"),
            ("Moda Sahnesi", "Istanbul", "sahne")
        };

        for (var i = 0; i < venueData.Length; i++)
        {
            dbContext.PopularVenues.Add(new PopularVenue
            {
                Name = venueData[i].Item1,
                City = venueData[i].Item2,
                Tag = venueData[i].Item3,
                SortOrder = i + 1,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
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
                    Body = "EventPazar, sehirdeki etkinlikleri tek bir pazaryerinde bulusturan bir platformdur. Konserlerden tiyatroya, festivallerden workshoplara kadar tüm etkinlikleri keşfedin.",
                    UpdatedAtUtc = DateTime.UtcNow
                },
                new SitePageContent
                {
                    Slug = "contact",
                    Title = "Iletisim",
                    Body = "Bize ulasmak icin hello@eventpazar.com adresine e-posta gonderebilirsiniz. Telefon: +90 212 555 00 00",
                    UpdatedAtUtc = DateTime.UtcNow
                },
                new SitePageContent
                {
                    Slug = "privacy",
                    Title = "Gizlilik Politikası",
                    Body = "Kullanici verileri KVKK ve ilgili mevzuat kapsaminda korunur. Kişisel verileriniz üçüncü taraflarla paylaşılmaz.",
                    UpdatedAtUtc = DateTime.UtcNow
                },
                new SitePageContent
                {
                    Slug = "terms",
                    Title = "Kullanım Koşulları",
                    Body = "EventPazar platformunu kullanarak aşağıdaki koşulları kabul etmiş sayılırsınız. Bilet satışları organizatör sorumluluğundadır.",
                    UpdatedAtUtc = DateTime.UtcNow
                },
                new SitePageContent
                {
                    Slug = "faq",
                    Title = "Sıkça Sorulan Sorular",
                    Body = "Bilet iade işlemleri etkinlikten 48 saat öncesine kadar yapılabilir. Detaylı bilgi için iletişim sayfamızı ziyaret edin.",
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
                },
                new AdvertisementSlot
                {
                    Placement = "listing-banner",
                    Title = "Listeleme Sayfası Banner",
                    Subtitle = "Etkinlik listesinde üst banner alanı",
                    Priority = 2,
                    IsActive = true
                },
                new AdvertisementSlot
                {
                    Placement = "footer-banner",
                    Title = "Footer Reklam Alanı",
                    Subtitle = "Sayfa altı geniş banner reklam",
                    Priority = 3,
                    IsActive = true
                });
        }

        if (!await dbContext.FeaturedListings.AnyAsync(cancellationToken))
        {
            var events = await dbContext.Events
                .Where(x => x.IsApproved)
                .OrderBy(x => x.StartDate)
                .Take(8)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            for (var i = 0; i < events.Count; i++)
            {
                dbContext.FeaturedListings.Add(new FeaturedListing
                {
                    EventId = events[i],
                    Placement = i < 4 ? "home-carousel" : "home-sidebar",
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
