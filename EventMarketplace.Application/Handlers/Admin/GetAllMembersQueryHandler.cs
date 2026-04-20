using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Queries.Admin;
using EventMarketplace.Application.Repositories;
using MediatR;

namespace EventMarketplace.Application.Handlers.Admin;

public sealed class GetAllMembersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetAllMembersQuery, IList<MemberDto>>
{
    public async Task<IList<MemberDto>> Handle(GetAllMembersQuery request, CancellationToken cancellationToken)
    {
        return await userRepository.GetAllAsync(cancellationToken);
    }
}
