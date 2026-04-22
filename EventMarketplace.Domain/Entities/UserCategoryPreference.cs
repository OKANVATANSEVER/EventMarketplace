namespace EventMarketplace.Domain.Entities;

public sealed class UserCategoryPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool WantsEmail { get; set; } = true;
    public bool WantsSms { get; set; } = false;

    public Category? Category { get; set; }
}
