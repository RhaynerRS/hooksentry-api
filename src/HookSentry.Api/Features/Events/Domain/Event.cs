namespace HookSentry.Api.Features.Events.Domain;

public class Event
{
    public virtual Guid Id { get; protected set; }
    public virtual Guid TenantId { get; protected set; }
    public virtual Guid DestinationUrlId { get; protected set; }
    public virtual string Payload { get; protected set; } = default!;
    public virtual EventStatus Status { get; protected set; }
    public virtual string? IdempotencyKey { get; protected set; }
    public virtual int CurrentRetryCount { get; protected set; }
    public virtual DateTimeOffset? NextAttemptAt { get; protected set; }
    public virtual DateTimeOffset AcceptedAt { get; protected set; }
    public virtual DateTimeOffset? DeliveredAt { get; protected set; }

    protected Event() { }

    public Event(Guid tenantId, Guid destinationUrlId, string payload, string? idempotencyKey = null)
    {
        SetTenantId(tenantId);
        SetDestinationUrlId(destinationUrlId);
        SetPayload(payload);
        SetIdempotencyKey(idempotencyKey);

        Id = Guid.NewGuid();
        Status = EventStatus.Pending;
        CurrentRetryCount = 0;
        AcceptedAt = DateTimeOffset.UtcNow;
    }

    public virtual void SetTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId não pode ser vazio.", nameof(tenantId));
        TenantId = tenantId;
    }

    public virtual void SetDestinationUrlId(Guid destinationUrlId)
    {
        if (destinationUrlId == Guid.Empty)
            throw new ArgumentException("DestinationUrlId não pode ser vazio.", nameof(destinationUrlId));
        DestinationUrlId = destinationUrlId;
    }

    public virtual void SetPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload não pode ser nulo ou vazio.", nameof(payload));

        try { System.Text.Json.JsonDocument.Parse(payload); }
        catch (System.Text.Json.JsonException)
        {
            throw new ArgumentException("Payload deve ser um JSON válido.", nameof(payload));
        }

        Payload = payload;
    }

    // RF-013: reprocessamento manual — somente eventos com status CriticalFailure
    public virtual void ResetForReplay()
    {
        if (Status != EventStatus.CriticalFailure)
            throw new InvalidOperationException(
                "Somente eventos com status 'CriticalFailure' podem ser reenviados.");

        CurrentRetryCount = 0;
        NextAttemptAt = DateTimeOffset.UtcNow;
        Status = EventStatus.Pending;
    }

    // RF-014: cancelamento em purga de fila
    public virtual void Cancel()
    {
        if (Status != EventStatus.Pending && Status != EventStatus.WaitingRetry)
            throw new InvalidOperationException(
                "Somente eventos com status 'Pending' ou 'WaitingRetry' podem ser cancelados.");

        Status = EventStatus.Cancelled;
    }

    private void SetIdempotencyKey(string? key)
    {
        if (key is not null && key.Length > 255)
            throw new ArgumentException(
                "IdempotencyKey não pode exceder 255 caracteres.", nameof(key));
        IdempotencyKey = key;
    }
}
