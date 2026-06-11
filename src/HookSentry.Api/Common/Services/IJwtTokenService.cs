using HookSentry.Api.Features.Users.Domain;

namespace HookSentry.Api.Common.Services;

public interface IJwtTokenService
{
    (string token, string jti, DateTimeOffset expiresAt) GenerateAccessToken(User user);
}
