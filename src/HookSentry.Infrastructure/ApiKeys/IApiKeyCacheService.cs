namespace HookSentry.Infrastructure.ApiKeys;

public sealed record ApiKeyCacheEntry(Guid TenantId);

public interface IApiKeyCacheService
{
    Task<ApiKeyCacheEntry?> GetAsync(string keyHash, CancellationToken ct = default);
    Task SetAsync(string keyHash, ApiKeyCacheEntry entry, CancellationToken ct = default);
    Task RemoveAsync(string keyHash, CancellationToken ct = default);
}
