using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.DataTransfer.Tenants.Requests;
using HookSentry.Api.DataTransfer.Tenants.Responses;
using HookSentry.Api.Features.Tenants.Domain;
using NHibernate;
using NHibernate.Linq;

namespace HookSentry.Api.Features.Tenants.CreateTenant;

public class CreateTenantEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/tenants", Handle)
            .WithName("CreateTenant")
            .WithTags("Tenants")
            .AllowAnonymous()
            .Produces<CreateTenantResponse>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateTenantRequest request,
        NHibernate.ISession session,
        CancellationToken ct)
    {
        var nameExists = await session.Query<Tenant>()
            .AnyAsync(t => t.Name == request.Name, ct);

        if (nameExists)
            return Results.Conflict($"Tenant '{request.Name}' already exists.");

        var tenant = new Tenant(request.Name, request.MaxTrys, request.CircuitBreakerTimer);

        using var tx = session.BeginTransaction();
        await session.SaveAsync(tenant, ct);
        await tx.CommitAsync(ct);

        return Results.Created(
            $"/api/v1/tenants/{tenant.Id}",
            new CreateTenantResponse(
                tenant.Id,
                tenant.Name,
                tenant.WebhookSecret,
                tenant.MaxTrys,
                tenant.CircuitBreakerTimer,
                tenant.CreatedAt));
    }
}

