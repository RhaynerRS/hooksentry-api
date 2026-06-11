namespace HookSentry.Api.DataTransfer.Tenants.Requests;

public record CreateTenantRequest(
    string Name,
    string AdminEmail,
    string AdminPassword,
    int MaxTrys = 10,
    int CircuitBreakerTimer = 300);
