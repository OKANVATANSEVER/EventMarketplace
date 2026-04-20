namespace EventMarketplace.Domain.Entities;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string City { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string OrganizerId { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
}