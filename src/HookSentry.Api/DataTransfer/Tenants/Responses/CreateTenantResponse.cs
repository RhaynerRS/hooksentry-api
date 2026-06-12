using HookSentry.Domain.Users;

namespace HookSentry.Api.DataTransfer.Tenants.Responses;

public record CreateTenantResponse(
    Guid TenantId,
    string TenantName,
    string WebhookSecret,
    int MaxTrys,
    int CircuitBreakerTimer,
    DateTimeOffset TenantCreatedAt,
    Guid AdminUserId,
    string AdminEmail,
    UserRole AdminRole,
    DateTimeOffset AdminCreatedAt);
