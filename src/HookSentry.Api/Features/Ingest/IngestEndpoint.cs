using System.Security.Claims;
using System.Text.Json;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;
using HookSentry.Api.Common.Validation;
using HookSentry.Api.DataTransfer.Events.Responses;
using HookSentry.Domain.Destinations;
using HookSentry.Domain.Events;
using HookSentry.Infrastructure.RabbitMq;
using NHibernate.Linq;

namespace HookSentry.Api.Features.Ingest;

public class IngestEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/ingest/{token}", Handle)
            .WithName("IngestEvent")
            .WithTags("Ingest")
            .WithSummary("Ingere um evento via ingest token")
            .WithDescription("""
                Recebe um payload JSON arbitrário e o persiste para entrega assíncrona na URL de destino
                associada ao ingest token informado.

                Configure a URL `https://{host}/api/v1/ingest/{ingestToken}` diretamente no serviço externo
                que emite os webhooks — sem necessidade de estruturar o payload.

                **Parâmetros de rota:**
                - `token` *(obrigatório)*: ingest token da URL de destino (obtido em `POST /api/v1/destinations`
                  ou `POST /api/v1/destinations/{id}/ingest-token`)

                **Headers:**
                - `Authorization` *(obrigatório)*: `Bearer <jwt>` com claim `tenant_id`
                - `X-Idempotency-Key` *(opcional)*: chave de até 255 caracteres — se já existir um evento
                  com a mesma chave para este tenant, retorna `200 OK` com os dados do evento original
                  sem reprocessar

                **Body:**
                - Objeto JSON arbitrário a ser entregue na URL de destino

                **Códigos de retorno:**
                - `202 Accepted`: evento aceito para entrega assíncrona
                - `200 OK`: evento duplicado retornado via idempotency key
                - `400 Bad Request`: payload inválido ou token malformado
                - `401 Unauthorized`: token JWT ausente ou inválido
                - `403 Forbidden`: ingest token pertence a outro tenant
                - `404 Not Found`: ingest token não encontrado
                - `422 Unprocessable Entity`: URL de destino inativa ou suspensa
                """)
            .RequireAuthorization()
            .Produces<EventAcceptedResponse>(StatusCodes.Status202Accepted)
            .Produces<EventAcceptedResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Handle(
        string token,
        [Microsoft.AspNetCore.Mvc.FromBody] JsonElement payload,
        ClaimsPrincipal user,
        HttpRequest httpRequest,
        NHibernate.ISession session,
        IEventPublisher publisher,
        CancellationToken ct)
    {
        if (user.RequireTenantId(out var tenantId) is { } err) return err;

        if (InputSanitizer.ValidateToken(token) is { } tokenErr)
            return Results.BadRequest(tokenErr);

        var idempotencyKey = httpRequest.Headers["X-Idempotency-Key"].FirstOrDefault();
        if (idempotencyKey is not null)
        {
            var existing = await session.Query<Event>()
                .Where(e => e.TenantId == tenantId && e.IdempotencyKey == idempotencyKey)
                .SingleOrDefaultAsync(ct);

            if (existing is not null)
                return Results.Ok(new EventAcceptedResponse(existing.Id, existing.Status.ToString(), existing.AcceptedAt));
        }

        var tokenHash = DestinationUrl.HashToken(token);
        var destination = await session.Query<DestinationUrl>()
            .Where(d => d.IngestTokenHash == tokenHash)
            .SingleOrDefaultAsync(ct);

        if (destination is null)
            return Results.NotFound("Ingest token não encontrado.");

        if (destination.TenantId != tenantId)
            return Results.Forbid();

        if (!destination.IsActive())
            return Results.UnprocessableEntity($"Destination '{destination.Id}' is not active.");

        Event evento;
        try
        {
            evento = new Event(tenantId, destination.Id, payload.GetRawText(), idempotencyKey);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }

        using var tx = session.BeginTransaction();
        await session.SaveAsync(evento, ct);
        await tx.CommitAsync(ct);

        await publisher.PublishAsync(new EventMessage(
            EventId: evento.Id,
            TenantId: evento.TenantId,
            DestinationUrlId: evento.DestinationUrlId,
            DestinationUrl: destination.Url,
            Payload: evento.Payload,
            RetryCount: evento.CurrentRetryCount,
            AuthType: destination.AuthType,
            CredentialsEncrypted: destination.CredentialsEncrypted
        ), ct);

        return Results.Accepted(
            $"/api/v1/events/{evento.Id}",
            new EventAcceptedResponse(evento.Id, evento.Status.ToString(), evento.AcceptedAt));
    }
}
