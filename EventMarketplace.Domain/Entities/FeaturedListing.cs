namespace EventMarketplace.Domain.Entities;

public sealed class FeaturedListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Placement { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ActiveSinceUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastStatusChangedAtUtc { get; set; } = DateTime.UtcNow;
    public long TotalActiveDurationSeconds { get; set; }

    public Event? Event { get; set; }
}
