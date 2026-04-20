using EventMarketplace.Application.DTOs;

namespace EventMarketplace.Application.Repositories;

public interface IUserRepository
{
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<IList<MemberDto>> GetMembersJoinedAfterAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IList<(string Email, string Name)>> GetAllMemberEmailsAsync(CancellationToken cancellationToken = default);
    Task<IList<MemberDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> SetActiveStatusAsync(string userId, bool isActive, CancellationToken cancellationToken = default);
    Task<(bool Success, string Error)> CreateMemberAsync(string firstName, string lastName, string email, string password, string role, CancellationToken cancellationToken = default);
}
