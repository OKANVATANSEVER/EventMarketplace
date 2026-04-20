using EventMarketplace.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.Infrastructure.Persistence.Seed;

internal static class IdentitySeed
{
    internal const string AdminRoleId = "b66e7108-1f5d-4b27-a5bf-a98ff4b3f2e1";
    internal const string OrganizerRoleId = "d6f2d3fd-37be-4715-b4a0-1dfa07749026";
    internal const string AdminUserId = "f8456f2d-ea47-492c-a892-5b6a44c917f9";

    // Pre-computed PasswordHasher v3 hash for "Admin123!"
    private const string AdminPasswordHash =
        "AQAAAAIAAYagAAAAEDq2owkK3iyjkqsnKRslKTQwOoej062hYwQsuiC+o18fqpG1y2grENKeMyMdL/vilw==";

    internal static void Seed(ModelBuilder builder)
    {
        var adminRole = new IdentityRole
        {
            Id = AdminRoleId,
            Name = "Admin",
            NormalizedName = "ADMIN"
        };

        var organizerRole = new IdentityRole
        {
            Id = OrganizerRoleId,
            Name = "Organizer",
            NormalizedName = "ORGANIZER"
        };

        var adminUser = new UserApp
        {
            Id = AdminUserId,
            UserName = "admin@eventmarketplace.com",
            NormalizedUserName = "ADMIN@EVENTMARKETPLACE.COM",
            Email = "admin@eventmarketplace.com",
            NormalizedEmail = "ADMIN@EVENTMARKETPLACE.COM",
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Admin",
            CreatedDate = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc),
            SecurityStamp = "54adf5a1-d8ad-4637-95ae-95f0cbc0f18d",
            ConcurrencyStamp = "fbdf3204-c014-48d8-b7dd-7f4767cf2d9e",
            PasswordHash = AdminPasswordHash
        };

        builder.Entity<IdentityRole>().HasData(adminRole);
        builder.Entity<IdentityRole>().HasData(organizerRole);
        builder.Entity<UserApp>().HasData(adminUser);
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = AdminRoleId,
                UserId = AdminUserId
            },
            new IdentityUserRole<string>
            {
                RoleId = OrganizerRoleId,
                UserId = AdminUserId
            });
    }
}
