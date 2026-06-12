using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HookSentry.Worker.Infrastructure.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HookSentry.Worker.Consumers;

public sealed class WebhookDeliveryConsumer(
    RabbitMqConnection mqConnection,
    IOptions<RabbitMqSettings> options,
    ILogger<WebhookDeliveryConsumer> logger) : BackgroundService
{
    private readonly RabbitMqConnection _mqConnection = mqConnection;
    private readonly string _exchange = options.Value.EventsExchange;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("WebhookDeliveryConsumer starting...");

        var channel = await _mqConnection.GetConnection().CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "webhooks.delivery",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: "webhooks.delivery",
            exchange: _exchange,
            routingKey: "tenant.#",
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<EventMessage>(ea.Body.Span);

                if (message is null)
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    return;
                }

                logger.LogInformation(
                    "Event received: {EventId} -> {DestinationUrl} (retry #{RetryCount})",
                    message.EventId, message.DestinationUrl, message.RetryCount);

                new HttpClient().PostAsync(message.DestinationUrl, new StringContent(message.Payload, Encoding.UTF8, "application/json"), stoppingToken).Wait(stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message with DeliveryTag {Tag}", ea.DeliveryTag);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "webhooks.delivery",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("WebhookDeliveryConsumer listening on 'webhooks.delivery'...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
