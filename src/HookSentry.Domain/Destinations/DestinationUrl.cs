namespace HookSentry.Domain.Destinations;

public class DestinationUrl
{
    public virtual Guid Id { get; protected set; }
    public virtual Guid TenantId { get; protected set; }
    public virtual string Url { get; protected set; } = default!;
    public virtual DestinationUrlStatus Status { get; protected set; }
    public virtual int ServerRateLimit { get; protected set; }
    public virtual DestinationAuthType? AuthType { get; protected set; }
    public virtual string? CredentialsEncrypted { get; protected set; }
    public virtual string? IngestTokenHash { get; protected set; }
    public virtual DateTimeOffset CreatedAt { get; protected set; }
    public virtual DateTimeOffset UpdatedAt { get; protected set; }

    protected DestinationUrl() { }

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

    public virtual bool IsActive() => Status == DestinationUrlStatus.Active;

    public virtual void Activate()
    {
        Status = DestinationUrlStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void Deactivate()
    {
        Status = DestinationUrlStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void Suspend()
    {
        Status = DestinationUrlStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void SetUrl(string url)
    {
        ValidateUrl(url);
        Url = url;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void SetTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId não pode ser vazio.", nameof(tenantId));

        TenantId = tenantId;
    }

    public virtual void SetServerRateLimit(int limit)
    {
        ValidateServerRateLimit(limit);
        ServerRateLimit = limit;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void SetAuth(DestinationAuthType? authType, string? credentialsEncrypted)
    {
        if (authType.HasValue && string.IsNullOrWhiteSpace(credentialsEncrypted))
            throw new ArgumentException(
                "Credenciais criptografadas são obrigatórias quando AuthType é definido.", nameof(credentialsEncrypted));

        if (!authType.HasValue && !string.IsNullOrWhiteSpace(credentialsEncrypted))
            throw new ArgumentException(
                "AuthType é obrigatório quando credenciais são fornecidas.", nameof(authType));

        AuthType = authType;
        CredentialsEncrypted = credentialsEncrypted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual string RotateIngestToken()
    {
        var rawBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToHexString(rawBytes).ToLowerInvariant();
        IngestTokenHash = HashToken(rawToken);
        UpdatedAt = DateTimeOffset.UtcNow;
        return rawToken;
    }

    public static string HashToken(string rawToken)
    {
        var hashBytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
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
