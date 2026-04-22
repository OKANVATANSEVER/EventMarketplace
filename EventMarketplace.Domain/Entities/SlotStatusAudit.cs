namespace EventMarketplace.Domain.Entities;

public sealed class SlotStatusAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SlotType { get; set; } = string.Empty;
    public Guid SlotId { get; set; }
    public bool PreviousIsActive { get; set; }
    public bool NewIsActive { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ChangedByUserId { get; set; }
    public string? ChangedByEmail { get; set; }
    public string? ChangedByRole { get; set; }
    public string? RequestIp { get; set; }
}