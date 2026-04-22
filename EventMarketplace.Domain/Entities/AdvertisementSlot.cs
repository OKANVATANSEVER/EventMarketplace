namespace EventMarketplace.Domain.Entities;

public sealed class AdvertisementSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Placement { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ActiveSinceUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastStatusChangedAtUtc { get; set; } = DateTime.UtcNow;
    public long TotalActiveDurationSeconds { get; set; }
    public int Priority { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
}
