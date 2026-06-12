using HookSentry.Domain.Destinations;

namespace HookSentry.Api.DataTransfer.Destinations.Responses;

public record CreateDestinationResponse(
    Guid Id,
    Guid TenantId,
    string Url,
    string Status,
    int ServerRateLimit,
    string? AuthType,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string IngestToken)
{
    public static CreateDestinationResponse From(DestinationUrl d, string ingestToken) =>
        new(d.Id, d.TenantId, d.Url, d.Status.ToString(), d.ServerRateLimit,
            d.AuthType?.ToString(), d.CreatedAt, d.UpdatedAt, ingestToken);
}
