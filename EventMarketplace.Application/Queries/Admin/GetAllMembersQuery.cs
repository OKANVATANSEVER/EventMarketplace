using EventMarketplace.Application.DTOs;
using MediatR;

namespace EventMarketplace.Application.Queries.Admin;

public sealed record GetAllMembersQuery : IRequest<IList<MemberDto>>;
