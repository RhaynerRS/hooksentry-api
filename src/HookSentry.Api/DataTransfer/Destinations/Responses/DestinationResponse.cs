using HookSentry.Api.Features.Destinations.Domain;

namespace HookSentry.Api.DataTransfer.Destinations.Responses;

public record DestinationResponse(
    Guid Id,
    Guid TenantId,
    string Url,
    string Status,
    int ServerRateLimit,
    string? AuthType,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static DestinationResponse From(DestinationUrl d) =>
        new(d.Id, d.TenantId, d.Url, d.Status.ToString(), d.ServerRateLimit,
            d.AuthType?.ToString(), d.CreatedAt, d.UpdatedAt);
}
