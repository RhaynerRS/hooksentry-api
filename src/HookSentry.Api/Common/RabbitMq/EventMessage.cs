namespace HookSentry.Api.Common.RabbitMq;

public sealed record EventMessage(
    Guid EventId,
    Guid TenantId,
    Guid DestinationUrlId,
    string DestinationUrl,
    string Payload,
    int RetryCount,
    int? AuthType,
    string? CredentialsEncrypted
);
