using EventMarketplace.Application.DTOs;

namespace EventMarketplace.Application.Repositories;

public interface IUserRepository
{
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<IList<MemberDto>> GetMembersJoinedAfterAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IList<(string Email, string Name)>> GetAllMemberEmailsAsync(CancellationToken cancellationToken = default);
}
