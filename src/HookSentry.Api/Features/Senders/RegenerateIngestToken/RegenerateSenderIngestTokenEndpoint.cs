using System.Security.Claims;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;
using HookSentry.Api.DataTransfer.Senders.Responses;
using HookSentry.Domain.Senders;

namespace HookSentry.Api.Features.Senders.RegenerateIngestToken;

public class RegenerateSenderIngestTokenEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/senders/{id:guid}/ingest-token", Handle)
            .WithName("RegenerateSenderIngestToken")
            .WithTags("Senders")
            .WithSummary("Regenera o ingest token de um sender")
            .WithDescription("""
                Gera um novo ingest token para o sender informado, invalidando o anterior imediatamente.

                O token retornado é exibido **uma única vez** — atualize a configuração do webhook
                no serviço externo antes de fechar esta resposta.

                **Parâmetros de rota:**
                - `id` *(obrigatório)*: UUID do sender

                **Códigos de retorno:**
                - `200 OK`: novo ingest token gerado
                - `401 Unauthorized`: token JWT ausente ou inválido
                - `403 Forbidden`: sender pertence a outro tenant
                - `404 Not Found`: sender não encontrado
                """)
            .RequireAuthorization()
            .Produces<SenderIngestTokenResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal user,
        NHibernate.ISession session,
        CancellationToken ct)
    {
        if (user.RequireTenantId(out var tenantId) is { } err) return err;

        using var tx = session.BeginTransaction();

        var sender = await session.GetAsync<WebhookSender>(id, ct);
        if (sender is null) return Results.NotFound();
        if (sender.TenantId != tenantId) return Results.Forbid();

        var rawToken = sender.RotateIngestToken();

        await tx.CommitAsync(ct);

        return Results.Ok(new SenderIngestTokenResponse(rawToken));
    }
}
