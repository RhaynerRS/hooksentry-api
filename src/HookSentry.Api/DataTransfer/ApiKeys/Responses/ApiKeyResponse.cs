using HookSentry.Domain.ApiKeys;

namespace HookSentry.Api.DataTransfer.ApiKeys.Responses;

public record ApiKeyResponse(
    Guid Id,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RevokedAt)
{
    public static ApiKeyResponse From(ApiKey k) =>
        new(k.Id, k.Name, k.IsActive, k.CreatedAt, k.RevokedAt);
}

public record ApiKeyCreatedResponse(
    Guid Id,
    string Name,
    string Key,
    DateTimeOffset CreatedAt);
