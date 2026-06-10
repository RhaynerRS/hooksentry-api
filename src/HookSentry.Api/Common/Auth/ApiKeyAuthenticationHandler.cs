using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HookSentry.Api.Common.Auth;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var providedKey = headerValues.ToString();
        var validKey = configuration["ApiKey:Value"];

        if (string.IsNullOrWhiteSpace(providedKey) || providedKey != validKey)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));

        var claims = new Claim[] { new(ClaimTypes.Name, "api-key-user") };
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)),
            Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
