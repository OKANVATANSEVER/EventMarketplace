using EventMarketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.Infrastructure.Persistence.Seed;

internal static class CategorySeed
{
    internal static readonly Guid ConcertCategoryId = Guid.Parse("5ef0db25-4218-42ff-b42f-41a72f16d1cd");
    internal static readonly Guid TheatreCategoryId = Guid.Parse("6116a3e5-d4ca-40c7-bf71-74d0ba034670");
    internal static readonly Guid FestivalCategoryId = Guid.Parse("89a8c1eb-c3ca-4cab-8953-675f7ec44ad7");

    internal static void Seed(ModelBuilder builder)
    {
        builder.Entity<Category>().HasData(
            new Category
            {
                Id = ConcertCategoryId,
                Name = "Concert",
                Description = "Live concerts and music performances"
            },
            new Category
            {
                Id = TheatreCategoryId,
                Name = "Theatre",
                Description = "Stage plays and theatre shows"
            },
            new Category
            {
                Id = FestivalCategoryId,
                Name = "Festival",
                Description = "Festivals and large public events"
            });
    }
}
