using System.Security.Claims;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.DataTransfer.Events.Responses;
using HookSentry.Api.Features.Events.Domain;
namespace HookSentry.Api.Features.Events.GetEvent;

public class GetEventEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/events/{id:guid}", Handle)
            .WithName("GetEvent")
            .WithTags("Events")
            .WithSummary("Retorna os detalhes de um evento pelo ID")
            .WithDescription("""
                Busca um evento pelo seu ID único de rastreamento.

                **Parâmetros de rota:**
                - `id` *(obrigatório)*: UUID do evento

                Retorna `403 Forbidden` se o evento pertencer a outro tenant (RNF-007).
                """)
            .RequireAuthorization()
            .Produces<EventResponse>()
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
        if (!Guid.TryParse(user.FindFirst("tenant_id")?.Value, out var tenantId))
            return Results.Unauthorized();

        var evento = await session.GetAsync<Event>(id, ct);

        if (evento is null)
            return Results.NotFound();

        // RNF-007: ownership check
        if (evento.TenantId != tenantId)
            return Results.Forbid();

        return Results.Ok(EventResponse.From(evento));
    }
}
