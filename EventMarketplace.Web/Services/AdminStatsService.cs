using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Queries.Admin;
using MediatR;

namespace EventMarketplace.Web.Services;

public sealed class AdminStatsService(IMediator mediator)
{
    public Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
        => mediator.Send(new GetAdminDashboardStatsQuery(), cancellationToken);
}
