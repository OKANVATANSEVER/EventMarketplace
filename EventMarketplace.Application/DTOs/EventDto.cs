namespace EventMarketplace.Application.DTOs;

public sealed record EventDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    Guid CategoryId,
    string CategoryName,
    string OrganizerId,
    bool IsFeatured,
    bool IsApproved);