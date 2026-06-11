using HookSentry.Api.Common.Security;
using HookSentry.Api.Common.Services;

namespace HookSentry.Api.Common.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
