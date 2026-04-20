using EventMarketplace.Web.Models;

namespace EventMarketplace.Web.Services;

public sealed class AdminStatsService(ApiClient api)
{
    public Task<AdminDashboardStatsModel?> GetDashboardStatsAsync(CancellationToken ct = default)
        => api.GetDashboardStatsAsync(ct);
}
