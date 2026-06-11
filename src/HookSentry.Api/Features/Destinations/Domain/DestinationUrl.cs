namespace HookSentry.Api.Features.Destinations.Domain;

public class DestinationUrl
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Url { get; private set; } = default!;
    public DestinationUrlStatus Status { get; private set; }
    public int ServerRateLimit { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private DestinationUrl() { }

    public DestinationUrl(Guid tenantId, string url, int serverRateLimit = 5)
    {
        SetTenantId(tenantId);
        ValidateUrl(url);
        ValidateServerRateLimit(serverRateLimit);

        Id = Guid.NewGuid();
        Url = url;
        ServerRateLimit = serverRateLimit;
        Status = DestinationUrlStatus.Active;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsActive() => Status == DestinationUrlStatus.Active;

    public void Activate()
    {
        Status = DestinationUrlStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        Status = DestinationUrlStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend()
    {
        Status = DestinationUrlStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetUrl(string url)
    {
        ValidateUrl(url);
        Url = url;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId não pode ser vazio.", nameof(tenantId));

        TenantId = tenantId;
    }

    public void SetServerRateLimit(int limit)
    {
        ValidateServerRateLimit(limit);
        ServerRateLimit = limit;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL não pode ser nula ou vazia.", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("URL deve ser um endereço HTTPS válido.", nameof(url));
    }

    private static void ValidateServerRateLimit(int limit)
    {
        if (limit < 1)
            throw new ArgumentOutOfRangeException(nameof(limit), "ServerRateLimit deve ser no mínimo 1.");
    }
}
