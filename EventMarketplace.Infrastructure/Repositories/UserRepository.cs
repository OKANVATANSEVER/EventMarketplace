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

        return await MapToMemberDtos(users);
    }

    public async Task<IList<MemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync(cancellationToken);

        return await MapToMemberDtos(users);
    }

    public async Task<bool> SetActiveStatusAsync(
        string userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user,
            isActive ? null : DateTimeOffset.MaxValue);
        return true;
    }

    public async Task<(bool Success, string Error)> CreateMemberAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = new UserApp
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(role))
            await userManager.AddToRoleAsync(user, role);

        return (true, string.Empty);
    }

    public async Task<IList<(string Email, string Name)>> GetAllMemberEmailsAsync(
        CancellationToken cancellationToken = default)
    {
        return await userManager.Users
            .Where(u => u.Email != null)
            .Select(u => ValueTuple.Create(u.Email!, u.FirstName + " " + u.LastName))
            .ToListAsync(cancellationToken);
    }

    private async Task<IList<MemberDto>> MapToMemberDtos(IList<UserApp> users)
    {
        var result = new List<MemberDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var isActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;
            result.Add(new MemberDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                user.CreatedDate,
                roles,
                isActive));
        }
        return result;
    }
}
