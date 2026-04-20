using EventMarketplace.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventMarketplace.API.Services;

public sealed class JwtTokenService(
    IConfiguration configuration,
    UserManager<UserApp> userManager)
{
    public async Task<(string AccessToken, DateTime ExpiresAtUtc)> CreateAccessTokenAsync(UserApp user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        var expiresMinutes = int.TryParse(jwtSection["AccessTokenExpirationMinutes"], out var parsed) ? parsed : 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        var roleNames = await userManager.GetRolesAsync(user);
        claims.AddRange(roleNames.Select(role => new Claim(ClaimTypes.Role, role)));

        var userClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return (accessToken, expiresAtUtc);
    }
}