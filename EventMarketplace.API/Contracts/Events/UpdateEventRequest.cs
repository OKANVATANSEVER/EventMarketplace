namespace EventMarketplace.API.Contracts.Events;

public sealed record UpdateEventRequest(
    string Title,
    string Description,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    Guid CategoryId,
    bool IsFeatured,
    bool IsApproved = false);
