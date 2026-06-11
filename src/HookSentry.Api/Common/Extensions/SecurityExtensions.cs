using HookSentry.Api.Common.Security;

namespace HookSentry.Api.Common.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        return services;
    }
}
