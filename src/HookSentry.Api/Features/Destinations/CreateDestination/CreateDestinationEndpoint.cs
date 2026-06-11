using System.Security.Claims;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.DataTransfer.Destinations.Requests;
using HookSentry.Api.DataTransfer.Destinations.Responses;
using HookSentry.Api.Features.Destinations.Domain;
using HookSentry.Api.Features.Tenants.Domain;

namespace HookSentry.Api.Features.Destinations.CreateDestination;

public class CreateDestinationEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/destinations", Handle)
            .WithName("CreateDestination")
            .WithTags("Destinations")
            .WithSummary("Cadastra uma nova URL de destino para o tenant autenticado")
            .WithDescription("""
                Registra uma URL HTTPS de destino para receber webhooks entregues pelo HookSentry.

                **Body:**
                - `url` *(obrigatório)*: URL HTTPS válida do endpoint de destino
                - `serverRateLimit` *(opcional, padrão: 5)*: número máximo de requisições simultâneas ao destino (RF-006)

                A URL é criada com status `Active`. O tenant autenticado é extraído do claim `tenant_id` do JWT.

                **Códigos de retorno:**
                - `201 Created`: URL de destino criada
                - `400 Bad Request`: URL inválida ou não-HTTPS
                - `401 Unauthorized`: token ausente ou inválido
                - `404 Not Found`: tenant não encontrado
                """)
            .RequireAuthorization()
            .Produces<DestinationResponse>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        CreateDestinationRequest request,
        ClaimsPrincipal user,
        NHibernate.ISession session,
        CancellationToken ct)
    {
        if (!Guid.TryParse(user.FindFirst("tenant_id")?.Value, out var tenantId))
            return Results.Unauthorized();

        var tenant = await session.GetAsync<Tenant>(tenantId, ct);
        if (tenant is null)
            return Results.NotFound($"Tenant '{tenantId}' not found.");

        DestinationUrl destination;
        try
        {
            destination = new DestinationUrl(tenantId, request.Url, request.ServerRateLimit);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }

        using var tx = session.BeginTransaction();
        await session.SaveAsync(destination, ct);
        await tx.CommitAsync(ct);

        return Results.Created(
            $"/api/v1/destinations/{destination.Id}",
            DestinationResponse.From(destination));
    }
}

