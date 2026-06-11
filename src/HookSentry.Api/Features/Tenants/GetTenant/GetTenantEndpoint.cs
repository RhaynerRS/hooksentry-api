using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.DataTransfer.Tenants.Responses;
using HookSentry.Api.Features.Tenants.Domain;
using NHibernate;

namespace HookSentry.Api.Features.Tenants.GetTenant;

public class GetTenantEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/tenants/{id:guid}", Handle)
            .WithName("GetTenantById")
            .WithTags("Tenants")
            .RequireAuthorization()
            .Produces<TenantResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        Guid id,
        NHibernate.ISession session,
        CancellationToken ct)
    {
        var tenant = await session.GetAsync<Tenant>(id, ct);

        return tenant is null
            ? Results.NotFound()
            : Results.Ok(new TenantResponse(
                tenant.Id,
                tenant.Name,
                tenant.MaxTrys,
                tenant.CircuitBreakerTimer,
                tenant.CreatedAt,
                tenant.UpdatedAt));
    }
}

