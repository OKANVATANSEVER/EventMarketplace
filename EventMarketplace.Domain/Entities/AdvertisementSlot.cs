namespace EventMarketplace.Domain.Entities;

public sealed class AdvertisementSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? CreatedByUserId { get; set; }
    public string Placement { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
}
