using HookSentry.Api.Features.Events.Domain;

namespace HookSentry.Api.DataTransfer.Events.Responses;

public record EventResponse(
    Guid Id,
    Guid TenantId,
    Guid DestinationUrlId,
    string Payload,
    string Status,
    string? IdempotencyKey,
    int CurrentRetryCount,
    DateTimeOffset? NextAttemptAt,
    DateTimeOffset AcceptedAt,
    DateTimeOffset? DeliveredAt)
{
    public static EventResponse From(Event e) =>
        new(e.Id, e.TenantId, e.DestinationUrlId, e.Payload, e.Status.ToString(),
            e.IdempotencyKey, e.CurrentRetryCount, e.NextAttemptAt, e.AcceptedAt, e.DeliveredAt);
}
