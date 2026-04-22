namespace EventMarketplace.Domain.Entities;

public sealed class EventComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = true;

    public Event? Event { get; set; }
}
