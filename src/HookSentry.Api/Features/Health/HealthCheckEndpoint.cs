using HookSentry.Api.Common.Endpoints;

namespace HookSentry.Api.Features.Health;

public class HealthCheckEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", Handle)
            .WithName("GetHealth")
            .WithTags("Health")
            .WithSummary("Verifica a saúde da API")
            .WithDescription("""
                Endpoint público de health check. Retorna o status da API e o timestamp UTC atual.

                **Não requer autenticação.**
                """)
            .AllowAnonymous()
            .Produces<HealthResponse>();
    }

    private static IResult Handle() =>
        Results.Ok(new HealthResponse("healthy", DateTime.UtcNow));
}

public record HealthResponse(string Status, DateTime Timestamp);
