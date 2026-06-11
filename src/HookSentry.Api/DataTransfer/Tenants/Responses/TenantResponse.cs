namespace HookSentry.Api.DataTransfer.Tenants.Responses;

public record TenantResponse(
    Guid Id,
    string Name,
    int MaxTrys,
    int CircuitBreakerTimer,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
