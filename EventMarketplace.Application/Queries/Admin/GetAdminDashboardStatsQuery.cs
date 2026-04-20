using EventMarketplace.Application.DTOs;
using MediatR;

namespace EventMarketplace.Application.Queries.Admin;

public record GetAdminDashboardStatsQuery : IRequest<AdminDashboardStatsDto>;
