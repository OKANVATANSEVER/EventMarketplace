using EventMarketplace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EventMarketplace.Infrastructure.Identity;

public class UserApp : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Event>? OrganizedEvents { get; set; }
    public ICollection<RefreshToken>? RefreshTokens { get; set; }
    public ICollection<UserCategoryPreference>? CategoryPreferences { get; set; }
}