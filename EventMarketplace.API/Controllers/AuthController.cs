using EventMarketplace.API.Contracts.Auth;
using EventMarketplace.API.Responses;
using EventMarketplace.API.Services;
using EventMarketplace.Infrastructure.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(
    UserManager<UserApp> userManager,
    JwtTokenService tokenService,
    RefreshTokenService refreshTokenService) : CustomBaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Email is already registered.", 409, true));

        var user = new UserApp
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ActionResultInstance(CustomResponse<NoContent>.Fail(new ErrorDto(errors, true), 400));
        }

        return ActionResultInstance(CustomResponse<object>.Success(new { message = "User registered successfully." }, 200));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Invalid credentials.", 401, true));

        var validPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Invalid credentials.", 401, true));

        var (accessToken, accessExpiry) = await tokenService.CreateAccessTokenAsync(user);
        var (refreshToken, refreshExpiry) = await refreshTokenService.GenerateAndSaveAsync(user, cancellationToken);

        var data = new AuthResponse(accessToken, accessExpiry, refreshToken, refreshExpiry);
        return ActionResultInstance(CustomResponse<AuthResponse>.Success(data, 200));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);
        if (result is null)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Invalid or expired refresh token.", 401, true));

        var (accessToken, accessExpiry, newRefreshToken, refreshExpiry) = result.Value;
        var data = new AuthResponse(accessToken, accessExpiry, newRefreshToken, refreshExpiry);
        return ActionResultInstance(CustomResponse<AuthResponse>.Success(data, 200));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeTokenRequest request,
        CancellationToken cancellationToken)
    {
        var revoked = await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        if (!revoked)
            return ActionResultInstance(CustomResponse<NoContent>.Fail("Refresh token not found or already revoked.", 404, true));

        return ActionResultInstance(CustomResponse<NoContent>.Success(200));
    }
}
