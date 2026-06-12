using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HookSentry.Infrastructure.RabbitMq;

public sealed class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _mqConnection;
    private readonly string _exchange;

    public RabbitMqEventPublisher(RabbitMqConnection mqConnection, IOptions<RabbitMqSettings> options)
    {
        _mqConnection = mqConnection;
        _exchange = options.Value.EventsExchange;
    }

    public async Task PublishAsync(EventMessage message, CancellationToken ct)
    {
        await using var channel = await _mqConnection.GetConnection().CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        var routingKey = $"tenant.{message.TenantId}.destination.{message.DestinationUrlId}";

        await channel.QueueDeclareAsync(
            queue: routingKey,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: routingKey,
            exchange: _exchange,
            routingKey: routingKey,
            cancellationToken: ct);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body,
            cancellationToken: ct);
    }
}
