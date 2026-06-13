using StackExchange.Redis;

namespace HookSentry.Infrastructure.ApiKeys;

public sealed class ApiKeyCacheService(IConnectionMultiplexer redis) : IApiKeyCacheService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<ApiKeyCacheEntry?> GetAsync(string keyHash, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(CacheKey(keyHash));
        if (!value.HasValue) return null;

        return Guid.TryParse(value.ToString(), out var tenantId)
            ? new ApiKeyCacheEntry(tenantId)
            : null;
    }

    public Task SetAsync(string keyHash, ApiKeyCacheEntry entry, CancellationToken ct = default)
        => _db.StringSetAsync(CacheKey(keyHash), entry.TenantId.ToString(), Ttl);

    public Task RemoveAsync(string keyHash, CancellationToken ct = default)
        => _db.KeyDeleteAsync(CacheKey(keyHash));

    private static string CacheKey(string hash) => $"apikey:{hash}";
}
