namespace HookSentry.Api.Features.Users.Domain;

public class User
{
    public virtual Guid Id { get; protected set; }
    public virtual Guid TenantId { get; protected set; }
    public virtual string Email { get; protected set; } = default!;
    public virtual string PasswordHash { get; protected set; } = default!;
    public virtual UserStatus Status { get; protected set; }
    public virtual UserRole Role { get; protected set; }
    public virtual DateTimeOffset CreatedAt { get; protected set; }
    public virtual DateTimeOffset UpdatedAt { get; protected set; }

    protected User() { }

    public User(Guid tenantId, string email, string passwordHash, UserRole role = UserRole.Developer)
    {
        SetTenantId(tenantId);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetRole(role);

        Id = Guid.NewGuid();
        Status = UserStatus.Active;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void SetTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId não pode ser vazio.", nameof(tenantId));
        TenantId = tenantId;
    }

    public virtual void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser nulo ou vazio.", nameof(email));
        if (email.Length > 255)
            throw new ArgumentException("Email não pode ultrapassar 255 caracteres.", nameof(email));
        if (!email.Contains('@'))
            throw new ArgumentException("Email deve ser um endereço válido.", nameof(email));
        Email = email.Trim().ToLowerInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash não pode ser nulo ou vazio.", nameof(passwordHash));
        PasswordHash = passwordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void SetRole(UserRole role)
    {
        if (!Enum.IsDefined<UserRole>(role))
            throw new ArgumentOutOfRangeException(nameof(role), "Role inválido.");
        Role = role;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public virtual void Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
