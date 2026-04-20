namespace EventMarketplace.Application.DTOs;

public sealed record EventListItemDto(
    Guid Id,
    string Title,
    decimal Price,
    DateTime StartDate,
    string City,
    string CategoryName,
    bool IsFeatured);
