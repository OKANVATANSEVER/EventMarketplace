using EventMarketplace.Application.DTOs;
using MediatR;
using EventMarketplace.Application.Repositories;

namespace EventMarketplace.Application.Queries.Admin;

public class GetAdminDashboardStatsQueryHandler(
    IUserRepository userRepository,
    IEventRepository eventRepository)
    : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    public async Task<AdminDashboardStatsDto> Handle(
        GetAdminDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var twelveMonthsAgo = now.AddMonths(-11);

        var totalMembers = await userRepository.GetTotalCountAsync(cancellationToken);
        var newMembers = await userRepository.GetMembersJoinedAfterAsync(startOfMonth, cancellationToken);
        var eventsThisMonth = await eventRepository.GetEventsStartingAfterAsync(startOfMonth, cancellationToken);
        var eventsByCategory = await eventRepository.GetEventCountByCategoryAsync(startOfMonth, cancellationToken);
        var monthlySummary = await eventRepository.GetMonthlyEventSummaryAsync(twelveMonthsAgo, cancellationToken);

        return new AdminDashboardStatsDto(
            totalMembers,
            newMembers.Count,
            newMembers,
            eventsThisMonth.Count,
            eventsByCategory,
            monthlySummary);
    }
}
