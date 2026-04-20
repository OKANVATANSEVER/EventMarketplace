namespace EventMarketplace.Web.Models;

public sealed record EventListItemModel(
    Guid Id,
    string Title,
    decimal Price,
    DateTime StartDate,
    string City,
    string CategoryName,
    bool IsFeatured);

public sealed record EventDetailModel(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate,
    string City,
    string CategoryName,
    bool IsFeatured);

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
