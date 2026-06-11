using System.Security.Cryptography;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;
using HookSentry.Api.Common.Services;
using HookSentry.Api.DataTransfer.Auth.Requests;
using HookSentry.Api.DataTransfer.Auth.Responses;
using HookSentry.Api.Features.Users.Domain;
using StackExchange.Redis;

namespace HookSentry.Api.Features.Auth.Refresh;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/refresh", Handle)
            .WithName("RefreshToken")
            .WithTags("Auth")
            .WithSummary("Renova o access token usando um refresh token válido")
            .WithDescription("""
                Troca um refresh token ativo por um novo par de tokens (access token + refresh token).
                O refresh token informado é consumido atomicamente (single-use via Redis GETDEL).

                **Body:**
                - `refreshToken` *(obrigatório)*: refresh token obtido no login ou em refresh anterior

                **Códigos de retorno:**
                - `200 OK`: novos tokens emitidos com sucesso
                - `400 Bad Request`: campo obrigatório ausente
                - `401 Unauthorized`: refresh token inválido, expirado ou já utilizado
                """)
            .AllowAnonymous()
            .Produces<AuthResponse>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        RefreshTokenRequest request,
        NHibernate.ISession session,
        IJwtTokenService jwtTokenService,
        IConnectionMultiplexer redis,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Results.BadRequest("RefreshToken é obrigatório.");

        var db = redis.GetDatabase();
        var value = await db.StringGetDeleteAsync($"{AuthExtensions.RefreshKeyPrefix}{request.RefreshToken}");

        if (!value.HasValue)
            return Results.Unauthorized();

        var parts = ((string)value!).Split('|');
        if (parts.Length != 2
            || !Guid.TryParse(parts[0], out var userId)
            || !Guid.TryParse(parts[1], out var tenantId))
            return Results.Unauthorized();

        var user = await session.GetAsync<User>(userId, ct);
        if (user is null || user.Status != UserStatus.Active || user.TenantId != tenantId)
            return Results.Unauthorized();

        var (accessToken, _, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        await db.StringSetAsync(
            $"{AuthExtensions.RefreshKeyPrefix}{newRefreshToken}",
            $"{user.Id}|{user.TenantId}",
            AuthExtensions.RefreshTokenTtl);

        return Results.Ok(new AuthResponse(
            accessToken,
            (int)(expiresAt - DateTimeOffset.UtcNow).TotalSeconds,
            newRefreshToken,
            expiresAt));
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
