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
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body,
            cancellationToken: ct);
    }

    public async Task PublishDelayedAsync(EventMessage message, int retryCount, CancellationToken ct)
    {
        await using var channel = await _mqConnection.GetConnection().CreateChannelAsync(cancellationToken: ct);
        var delayQueue = GetDelayQueue(retryCount);
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: delayQueue,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body,
            cancellationToken: ct);
    }

    private static string GetDelayQueue(int retryCount) => retryCount switch
    {
        1 => "hooksentry.delay.2m",
        2 => "hooksentry.delay.5m",
        3 => "hooksentry.delay.15m",
        4 => "hooksentry.delay.1h",
        _ => "hooksentry.delay.6h"
    };
}
