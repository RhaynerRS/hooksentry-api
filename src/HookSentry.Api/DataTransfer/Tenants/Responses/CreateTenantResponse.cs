namespace HookSentry.Api.DataTransfer.Tenants.Responses;

public record CreateTenantResponse(
    Guid Id,
    string Name,
    string WebhookSecret,
    int MaxTrys,
    int CircuitBreakerTimer,
    DateTimeOffset CreatedAt);
