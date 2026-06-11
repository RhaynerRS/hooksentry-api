using System.Security.Claims;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;
using HookSentry.Api.DataTransfer.Destinations.Requests;
using HookSentry.Api.DataTransfer.Destinations.Responses;
using HookSentry.Api.Features.Destinations.Domain;

namespace HookSentry.Api.Features.Destinations.UpdateDestination;

public class UpdateDestinationEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/v1/destinations/{id:guid}", Handle)
            .WithName("UpdateDestination")
            .WithTags("Destinations")
            .WithSummary("Atualiza campos de uma URL de destino")
            .WithDescription("""
                Atualiza parcialmente uma URL de destino pertencente ao tenant autenticado.

                **Parâmetros de rota:**
                - `id` *(obrigatório)*: UUID da URL de destino

                **Body** *(todos os campos são opcionais):*
                - `url`: nova URL HTTPS válida
                - `serverRateLimit`: novo limite de requisições simultâneas (mínimo: 1)
                - `status`: `active` ou `inactive` — o valor `suspended` é gerenciado exclusivamente
                  pelo Circuit Breaker (RF-011) e não pode ser definido manualmente

                **Códigos de retorno:**
                - `200 OK`: URL de destino atualizada
                - `400 Bad Request`: valor inválido ou status não permitido
                - `401 Unauthorized`: token ausente ou inválido
                - `403 Forbidden`: URL de destino pertence a outro tenant (RNF-007)
                - `404 Not Found`: URL de destino não encontrada
                """)
            .RequireAuthorization()
            .Produces<DestinationResponse>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        UpdateDestinationRequest request,
        ClaimsPrincipal user,
        NHibernate.ISession session,
        CancellationToken ct)
    {
        if (user.RequireTenantId(out var tenantId) is { } err) return err;

        using var tx = session.BeginTransaction();

        var destination = await session.GetAsync<DestinationUrl>(id, ct);

        if (destination is null)
            return Results.NotFound();

        if (destination.TenantId != tenantId)
            return Results.Forbid();

        try
        {
            if (request.Url is not null)
                destination.SetUrl(request.Url);

            if (request.ServerRateLimit.HasValue)
                destination.SetServerRateLimit(request.ServerRateLimit.Value);

            if (request.Status is not null)
            {
                switch (request.Status.ToLowerInvariant())
                {
                    case "active":
                        destination.Activate();
                        break;
                    case "inactive":
                        destination.Deactivate();
                        break;
                    case "suspended":
                        return Results.BadRequest(
                            "Status 'suspended' is managed by the circuit breaker (RF-011). Use 'active' or 'inactive'.");
                    default:
                        return Results.BadRequest(
                            $"Invalid status '{request.Status}'. Valid values: 'active', 'inactive'.");
                }
            }
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }

        await tx.CommitAsync(ct);

        return Results.Ok(DestinationResponse.From(destination));
    }
}

