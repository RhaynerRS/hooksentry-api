using System.Security.Cryptography;

namespace HookSentry.Api.Features.Tenants.Domain;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string WebhookSecret { get; private set; } = default!;
    public int MaxTrys { get; private set; }
    public int CircuitBreakerTimer { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Tenant() { }

    public Tenant(string name, int maxTrys = 10, int circuitBreakerTimer = 300)
    {
        Id = Guid.NewGuid();
        Name = name;
        WebhookSecret = GenerateWebhookSecret();
        MaxTrys = maxTrys;
        CircuitBreakerTimer = circuitBreakerTimer;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateSettings(int maxTrys, int circuitBreakerTimer)
    {
        MaxTrys = maxTrys;
        CircuitBreakerTimer = circuitBreakerTimer;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RotateWebhookSecret()
    {
        WebhookSecret = GenerateWebhookSecret();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string GenerateWebhookSecret() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
}
