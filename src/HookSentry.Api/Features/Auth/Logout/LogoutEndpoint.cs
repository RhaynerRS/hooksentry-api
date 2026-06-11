using System.Security.Claims;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;
using HookSentry.Api.DataTransfer.Auth.Requests;
using StackExchange.Redis;

namespace HookSentry.Api.Features.Auth.Logout;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/logout", Handle)
            .WithName("Logout")
            .WithTags("Auth")
            .WithSummary("Invalida o refresh token do usuário autenticado")
            .WithDescription("""
                Remove o refresh token do Redis, impedindo que ele seja reutilizado.
                O access token (JWT) permanece válido até expirar naturalmente (15 minutos).

                **Body:**
                - `refreshToken` *(obrigatório)*: refresh token a ser invalidado

                **Códigos de retorno:**
                - `204 No Content`: logout realizado com sucesso
                - `400 Bad Request`: refreshToken ausente
                - `401 Unauthorized`: token JWT ausente ou inválido, ou refresh token não pertence ao usuário
                """)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Handle(
        LogoutRequest request,
        ClaimsPrincipal user,
        IConnectionMultiplexer redis,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Results.BadRequest("RefreshToken é obrigatório.");

        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("sub")?.Value, out var userId))
            return Results.Unauthorized();

        var db = redis.GetDatabase();
        var key = $"{AuthExtensions.RefreshKeyPrefix}{request.RefreshToken}";
        var value = await db.StringGetAsync(key);

        if (!value.HasValue)
            return Results.Unauthorized();

        var parts = ((string)value!).Split('|');
        if (parts.Length < 1 || !Guid.TryParse(parts[0], out var tokenOwner) || tokenOwner != userId)
            return Results.Unauthorized();

        await db.KeyDeleteAsync(key);
        return Results.NoContent();
    }
}
