namespace EventMarketplace.Application.Queries.Events;

public sealed record GetEventsFilter(
    string? City = null,
    Guid? CategoryId = null,
    bool? IsFeatured = null,
    bool? IsApproved = null,
    string? Title = null,
    int Page = 1,
    int PageSize = 20);
