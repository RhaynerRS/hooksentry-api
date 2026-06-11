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

