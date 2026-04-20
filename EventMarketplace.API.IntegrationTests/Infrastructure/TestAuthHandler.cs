using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EventMarketplace.API.IntegrationTests.Infrastructure;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out var userHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var role = Request.Headers.TryGetValue("X-Test-Role", out var roleHeader)
            ? roleHeader.ToString()
            : "Customer";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userHeader.ToString()),
            new(ClaimTypes.Name, userHeader.ToString()),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
