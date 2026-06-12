using HookSentry.Domain.Destinations;

namespace HookSentry.Infrastructure.RabbitMq;

public sealed record EventMessage(
    Guid EventId,
    Guid TenantId,
    Guid DestinationUrlId,
    string DestinationUrl,
    string Payload,
    int RetryCount,
    DestinationAuthType? AuthType,
    string? CredentialsEncrypted
);
