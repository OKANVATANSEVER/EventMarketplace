namespace EventMarketplace.Domain.Entities;

public sealed class FeaturedListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string Placement { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public Event? Event { get; set; }
}
