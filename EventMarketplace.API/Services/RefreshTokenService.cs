using EventMarketplace.Infrastructure.Identity;
using EventMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EventMarketplace.API.Services;

public sealed class RefreshTokenService(
    ApplicationDbContext dbContext,
    JwtTokenService jwtTokenService,
    IConfiguration configuration)
{
    private int ExpirationDays =>
        int.TryParse(configuration["Jwt:RefreshTokenExpirationDays"], out var d) ? d : 7;

    public async Task<(string Token, DateTime ExpiresAt)> GenerateAndSaveAsync(
        UserApp user,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTime.UtcNow.AddDays(ExpirationDays);

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = user.Id,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (refreshToken.Token, expiresAt);
    }

    public async Task<(string AccessToken, DateTime AccessExpiry, string NewRefreshToken, DateTime RefreshExpiry)?> RotateAsync(
        string oldToken,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == oldToken, cancellationToken);

        if (existing is null || existing.IsRevoked || existing.ExpiresAt <= DateTime.UtcNow)
            return null;

        // Revoke old token (token rotation)
        existing.IsRevoked = true;

        var newTokenValue = GenerateSecureToken();
        existing.ReplacedByToken = newTokenValue;

        var newRefreshExpiry = DateTime.UtcNow.AddDays(ExpirationDays);
        var newRefreshToken = new RefreshToken
        {
            Token = newTokenValue,
            UserId = existing.UserId,
            ExpiresAt = newRefreshExpiry,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpiry) = await jwtTokenService.CreateAccessTokenAsync(existing.User);
        return (accessToken, accessExpiry, newTokenValue, newRefreshExpiry);
    }

    public async Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

        if (existing is null || existing.IsRevoked)
            return false;

        existing.IsRevoked = true;

        // Revoke all chained descendants to prevent replay with rotated tokens.
        var toRevoke = new Queue<string>();
        if (!string.IsNullOrWhiteSpace(existing.ReplacedByToken))
            toRevoke.Enqueue(existing.ReplacedByToken);

        while (toRevoke.Count > 0)
        {
            var currentToken = toRevoke.Dequeue();
            var child = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == currentToken, cancellationToken);

            if (child is null)
                continue;

            child.IsRevoked = true;
            if (!string.IsNullOrWhiteSpace(child.ReplacedByToken))
                toRevoke.Enqueue(child.ReplacedByToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
