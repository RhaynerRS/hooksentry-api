using HookSentry.Domain.Senders;

namespace HookSentry.Api.DataTransfer.Senders.Responses;

public record CreateSenderResponse(
    Guid Id,
    Guid DestinationId,
    Guid TenantId,
    string? Label,
    bool HasMapping,
    string IngestToken,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static CreateSenderResponse From(WebhookSender s, string ingestToken) =>
        new(s.Id, s.DestinationId, s.TenantId, s.Label, s.Mapping is not null,
            ingestToken, s.CreatedAt, s.UpdatedAt);
}
