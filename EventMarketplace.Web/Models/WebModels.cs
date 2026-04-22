namespace EventMarketplace.Web.Models;

public sealed record EventListItemModel(
    Guid Id,
    string Title,
    decimal Price,
    DateTime StartDate,
    string City,
    string CategoryName,
    bool IsFeatured,
    bool IsApproved);

public sealed record EventDetailModel(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    string CategoryName,
    bool IsFeatured,
    bool IsApproved);

public sealed record PagedResultModel<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record MemberModel(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedDate,
    IList<string> Roles,
    bool IsActive = true);

public sealed record CategoryModel(Guid Id, string Name);

public sealed record CategoryEventCountModel(
    string CategoryName,
    int EventCount);

public sealed record MonthlySummaryModel(
    int Year,
    int Month,
    string MonthName,
    int EventCount);

public sealed record AdminDashboardStatsModel(
    int TotalMembers,
    int NewMembersThisMonth,
    IList<MemberModel> NewMembers,
    int TotalEventsThisMonth,
    IList<CategoryEventCountModel> EventsByCategory,
    IList<MonthlySummaryModel> MonthlyEventSummary);

public sealed record AuthTokensModel(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record ApiEnvelope<T>(
    T? Data,
    int StatusCode,
    bool IsSuccessful);

public sealed record SitePageContentModel(
    string Slug,
    string Title,
    string Body,
    DateTime UpdatedAtUtc);

public sealed record AdvertisementSlotModel(
    Guid Id,
    string Placement,
    string Title,
    string Subtitle,
    string? LinkUrl,
    string? ImageUrl,
    bool IsActive,
    int Priority,
    DateTime? StartDateUtc,
    DateTime? EndDateUtc);

public sealed record FeaturedListingModel(
    Guid Id,
    Guid EventId,
    string EventTitle,
    string Placement,
    int Priority,
    DateTime? ExpiresAtUtc,
    bool IsActive);

public sealed record PublicFeaturedEventModel(
    Guid EventId,
    string EventTitle,
    string City,
    DateTime StartDate,
    decimal Price,
    string CategoryName);

public sealed record EventCommentModel(
    Guid Id,
    string DisplayName,
    string Content,
    DateTime CreatedAtUtc);

public sealed record EventGalleryItemModel(
    Guid Id,
    string ImageUrl,
    string? Caption,
    int SortOrder);

public sealed record EventInteractionsModel(
    List<EventCommentModel> Comments,
    List<EventGalleryItemModel> Gallery);

public sealed record AccountProfileModel(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    List<string> Roles);

public sealed record NotificationPreferenceModel(
    Guid CategoryId,
    string CategoryName,
    bool WantsEmail,
    bool WantsSms);

public sealed record OrganizerEventModel(
    Guid Id,
    string Title,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    Guid CategoryId,
    string CategoryName,
    bool IsFeatured,
    bool IsApproved);

public sealed record AdminInboxMessageModel(
    Guid Id,
    Guid EventId,
    string EventTitle,
    string SenderName,
    string SenderEmail,
    string MessageText,
    DateTime CreatedAtUtc);
