namespace HookSentry.Api.Common.RabbitMq;

public interface IEventPublisher
{
    Task PublishAsync(EventMessage message, CancellationToken ct);
}
