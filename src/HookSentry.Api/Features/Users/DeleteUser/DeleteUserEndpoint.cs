using System.Security.Claims;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Features.Users.Domain;

namespace HookSentry.Api.Features.Users.DeleteUser;

public class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/users/{id:guid}", Handle)
            .WithName("DeleteUser")
            .WithTags("Users")
            .WithSummary("Remove permanentemente um usuário")
            .WithDescription("""
                Exclui definitivamente um usuário do tenant autenticado.
                Operação irreversível — use com confirmação explícita no frontend.

                **Restrição de perfil:** apenas usuários com `role = Admin` podem executar esta operação.
                O claim `role` do token JWT é usado para verificar o perfil — nunca o body (RNF-007).

                **Parâmetros de rota:**
                - `id` *(obrigatório)*: UUID do usuário a ser excluído

                **Códigos de retorno:**
                - `204 No Content`: usuário excluído com sucesso
                - `401 Unauthorized`: token ausente, inválido ou sem claim `role`
                - `403 Forbidden`: usuário autenticado não é Admin, ou o alvo pertence a outro tenant (RNF-007)
                - `404 Not Found`: usuário não encontrado
                """)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal principal,
        NHibernate.ISession session,
        CancellationToken ct)
    {
        if (!Guid.TryParse(principal.FindFirst("tenant_id")?.Value, out var tenantId))
            return Results.Unauthorized();

        if (!Enum.TryParse<UserRole>(principal.FindFirst("role")?.Value, ignoreCase: true, out var callerRole))
            return Results.Unauthorized();

        if (callerRole != UserRole.Admin)
            return Results.Forbid();

        using var tx = session.BeginTransaction();

        var user = await session.GetAsync<User>(id, ct);

        if (user is null) return Results.NotFound();
        if (user.TenantId != tenantId) return Results.Forbid();

        await session.DeleteAsync(user, ct);
        await tx.CommitAsync(ct);

        return Results.NoContent();
    }
}
