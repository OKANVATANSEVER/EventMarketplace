using EventMarketplace.API.Responses;
using EventMarketplace.Domain.Entities;
using EventMarketplace.Infrastructure.Identity;
using EventMarketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController(UserManager<UserApp> userManager, ApplicationDbContext dbContext) : CustomBaseController
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 401, true));

        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 404, true));

        var roles = await userManager.GetRolesAsync(user);

        return ActionResultInstance(CustomResponse<object>.Success(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            Roles = roles
        }, 200));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 401, true));

        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 404, true));

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Profile update failed.", 400, true));

        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 401, true));

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 404, true));

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Password change failed.", 400, true));

        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 401, true));

        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync(cancellationToken);

        var existing = await dbContext.UserCategoryPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var data = categories.Select(c =>
        {
            var pref = existing.FirstOrDefault(x => x.CategoryId == c.Id);
            return new
            {
                categoryId = c.Id,
                categoryName = c.Name,
                wantsEmail = pref?.WantsEmail ?? false,
                wantsSms = pref?.WantsSms ?? false
            };
        }).ToList();

        return ActionResultInstance(CustomResponse<object>.Success(data, 200));
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> SavePreferences([FromBody] List<SavePreferenceRequest> request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return ActionResultInstance(CustomResponse<NoContent>.Fail("User not found.", 401, true));

        var current = await dbContext.UserCategoryPreferences
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        dbContext.UserCategoryPreferences.RemoveRange(current);

        var rows = request.Select(x => new UserCategoryPreference
        {
            UserId = userId,
            CategoryId = x.CategoryId,
            WantsEmail = x.WantsEmail,
            WantsSms = x.WantsSms
        });

        await dbContext.UserCategoryPreferences.AddRangeAsync(rows, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }
}

public record UpdateProfileRequest(string FirstName, string LastName, string? PhoneNumber);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record SavePreferenceRequest(Guid CategoryId, bool WantsEmail, bool WantsSms);
