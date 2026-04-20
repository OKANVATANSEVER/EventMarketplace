namespace EventMarketplace.Application.DTOs;

public record MemberDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedDate,
    IList<string> Roles);

public record CategoryEventCountDto(
    string CategoryName,
    int EventCount);

public record AdminDashboardStatsDto(
    int TotalMembers,
    int NewMembersThisMonth,
    IList<MemberDto> NewMembers,
    int TotalEventsThisMonth,
    IList<CategoryEventCountDto> EventsByCategory,
    IList<MonthlySummaryDto> MonthlyEventSummary);

public record MonthlySummaryDto(
    int Year,
    int Month,
    string MonthName,
    int EventCount);
