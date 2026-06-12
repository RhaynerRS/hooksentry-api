using System.Security.Claims;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;
using HookSentry.Api.Common.RabbitMq;
using HookSentry.Api.DataTransfer.Events.Requests;
using HookSentry.Api.DataTransfer.Events.Responses;
using HookSentry.Api.Features.Destinations.Domain;
using HookSentry.Api.Features.Events.Domain;
using NHibernate.Linq;

namespace HookSentry.Api.Features.Events.CreateEvent;

public class CreateEventEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/events", Handle)
            .WithName("CreateEvent")
            .WithTags("Events")
            .WithSummary("Ingere um novo evento para entrega assíncrona")
            .WithDescription("""
                Recebe um payload JSON e persiste o evento para entrega na URL de destino informada.

                **Headers:**
                - `Authorization` *(obrigatório)*: `Bearer <jwt>` com claim `tenant_id`
                - `X-Idempotency-Key` *(opcional)*: chave de até 255 caracteres — se já existir um evento
                  com a mesma chave para este tenant, retorna `200 OK` com os dados do evento original
                  sem reprocessar (RNF-005)

                **Body:**
                - `destinationUrlId` *(obrigatório)*: UUID da URL de destino cadastrada e ativa
                - `payload` *(obrigatório)*: objeto JSON arbitrário a ser entregue

                **Validações da URL de destino (RF-003):**
                - Não encontrada → `404`
                - Pertence a outro tenant → `403`
                - Status diferente de `Active` → `422`
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
        CreateEventRequest request,
        ClaimsPrincipal user,
        HttpRequest httpRequest,
        NHibernate.ISession session,
        IEventPublisher publisher,
        CancellationToken ct)
    {
        if (user.RequireTenantId(out var tenantId) is { } err) return err;

        var idempotencyKey = httpRequest.Headers["X-Idempotency-Key"].FirstOrDefault();
        if (idempotencyKey is not null)
        {
            var existing = await session.Query<Event>()
                .Where(e => e.TenantId == tenantId && e.IdempotencyKey == idempotencyKey)
                .SingleOrDefaultAsync(ct);

            if (existing is not null)
                return Results.Ok(new EventAcceptedResponse(existing.Id, existing.Status.ToString(), existing.AcceptedAt));
        }

        var destination = await session.GetAsync<DestinationUrl>(request.DestinationUrlId, ct);

        if (destination is null)
            return Results.NotFound($"Destination '{request.DestinationUrlId}' not found.");

        if (destination.TenantId != tenantId)
            return Results.Forbid();

        if (!destination.IsActive())
            return Results.UnprocessableEntity($"Destination '{request.DestinationUrlId}' is not active.");

        Event evento;
        try
        {
            evento = new Event(tenantId, request.DestinationUrlId, request.Payload.GetRawText(), idempotencyKey);
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
            AuthType: destination.AuthType.HasValue ? (int)destination.AuthType.Value : null,
            CredentialsEncrypted: destination.CredentialsEncrypted
        ), ct);

        return Results.Accepted(
            $"/api/v1/events/{evento.Id}",
            new EventAcceptedResponse(evento.Id, evento.Status.ToString(), evento.AcceptedAt));
    }
}
