using HookSentry.Domain.Senders;

namespace HookSentry.Api.DataTransfer.Senders.Responses;

public record SenderResponse(
    Guid Id,
    Guid DestinationId,
    Guid TenantId,
    string? Label,
    bool HasMapping,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static SenderResponse From(WebhookSender s) =>
        new(s.Id, s.DestinationId, s.TenantId, s.Label, s.Mapping is not null, s.CreatedAt, s.UpdatedAt);
}
