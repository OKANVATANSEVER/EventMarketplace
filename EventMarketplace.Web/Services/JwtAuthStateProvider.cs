using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EventMarketplace.Web.Services;

public sealed class JwtAuthStateProvider(TokenStore tokenStore, ILocalStorageService localStorage)
    : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    // JWT short-name → .NET ClaimTypes mapping
    private static readonly Dictionary<string, string> ClaimTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nameid"]      = ClaimTypes.NameIdentifier,
        ["sub"]         = ClaimTypes.NameIdentifier,
        ["unique_name"] = ClaimTypes.Name,
        ["role"]        = ClaimTypes.Role,
        ["email"]       = ClaimTypes.Email,
    };

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!tokenStore.IsAuthenticated)
            return Task.FromResult(Anonymous);

        var identity = new ClaimsIdentity(tokenStore.Claims, "jwt",
            ClaimTypes.Name, ClaimTypes.Role);
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public async Task LoginAsync(string token)
    {
        var claims = ParseClaims(token);
        tokenStore.Set(token, claims);
        tokenStore.MarkInitialized();
        await localStorage.SetItemAsStringAsync("em_token", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        tokenStore.Clear();
        await localStorage.RemoveItemAsync("em_token");
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public async Task InitializeFromStorageAsync()
    {
        if (tokenStore.IsInitialized) return;
        try
        {
            var token = await localStorage.GetItemAsStringAsync("em_token");
            if (!string.IsNullOrEmpty(token) && !IsTokenExpired(token))
            {
                var claims = ParseClaims(token);
                tokenStore.Set(token, claims);
            }
        }
        catch { /* LocalStorage not available during SSR prerender */ }

        tokenStore.MarkInitialized();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static bool IsTokenExpired(string token)
    {
        var claims = ParseClaims(token);
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (expClaim is null || !long.TryParse(expClaim, out var exp)) return true;
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= exp;
    }

    public static IEnumerable<Claim> ParseClaims(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return [];

        var payload = parts[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        payload = (payload.Length % 4) switch
        {
            2 => payload + "==",
            3 => payload + "=",
            _ => payload
        };

        var bytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(bytes);

        using var doc = JsonDocument.Parse(json);
        var claims = new List<Claim>();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            var claimType = ClaimTypeMap.TryGetValue(prop.Name, out var mapped) ? mapped : prop.Name;
            if (prop.Value.ValueKind == JsonValueKind.Array)
                foreach (var item in prop.Value.EnumerateArray())
                    claims.Add(new Claim(claimType, item.GetString() ?? string.Empty));
            else
                claims.Add(new Claim(claimType, prop.Value.ToString()));
        }
        return claims;
    }
}
