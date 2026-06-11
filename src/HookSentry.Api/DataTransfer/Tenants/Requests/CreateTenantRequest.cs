namespace HookSentry.Api.DataTransfer.Tenants.Requests;

public record CreateTenantRequest(
    string Name,
    int MaxTrys = 10,
    int CircuitBreakerTimer = 300);
