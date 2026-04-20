using System.Security.Claims;

namespace EventMarketplace.Web.Services;

/// <summary>Scoped per Blazor Server circuit — holds the JWT in memory.</summary>
public sealed class TokenStore
{
    public string? AccessToken { get; private set; }
    public bool IsInitialized { get; private set; }

    private readonly List<Claim> _claims = new();
    public IReadOnlyList<Claim> Claims => _claims;
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public void Set(string token, IEnumerable<Claim> claims)
    {
        AccessToken = token;
        _claims.Clear();
        _claims.AddRange(claims);
    }

    public void Clear()
    {
        AccessToken = null;
        _claims.Clear();
    }

    public void MarkInitialized() => IsInitialized = true;
}
