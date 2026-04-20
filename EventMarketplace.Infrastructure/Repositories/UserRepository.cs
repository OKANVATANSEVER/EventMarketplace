using EventMarketplace.Application.DTOs;
using EventMarketplace.Application.Repositories;
using EventMarketplace.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventMarketplace.Infrastructure.Repositories;

public sealed class UserRepository(UserManager<UserApp> userManager) : IUserRepository
{
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await userManager.Users.CountAsync(cancellationToken);
    }

    public async Task<IList<MemberDto>> GetMembersJoinedAfterAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .Where(u => u.CreatedDate >= date)
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync(cancellationToken);

        var result = new List<MemberDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new MemberDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                user.CreatedDate,
                roles));
        }
        return result;
    }

    public async Task<IList<(string Email, string Name)>> GetAllMemberEmailsAsync(
        CancellationToken cancellationToken = default)
    {
        return await userManager.Users
            .Where(u => u.Email != null)
            .Select(u => ValueTuple.Create(u.Email!, u.FirstName + " " + u.LastName))
            .ToListAsync(cancellationToken);
    }
}
