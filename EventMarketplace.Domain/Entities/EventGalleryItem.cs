namespace EventMarketplace.Domain.Entities;

public sealed class EventGalleryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }

    public Event? Event { get; set; }
}
