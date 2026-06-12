namespace HookSentry.Infrastructure.RabbitMq;

public interface IEventPublisher
{
    Task PublishAsync(EventMessage message, CancellationToken ct);
    Task PublishDelayedAsync(EventMessage message, int retryCount, CancellationToken ct);
}
