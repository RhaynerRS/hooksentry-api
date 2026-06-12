using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HookSentry.Worker.Infrastructure.RabbitMq;
using HookSentry.Worker.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HookSentry.Worker.Consumers;

public sealed class WebhookDeliveryConsumer(
    RabbitMqConnection mqConnection,
    IOptions<RabbitMqSettings> options,
    AesCredentialDecryptionService decryption,
    NpgsqlDataSource db,
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
            EventMessage? message = null;
            try
            {
                message = JsonSerializer.Deserialize<EventMessage>(ea.Body.Span);

                if (message is null)
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    return;
                }

                logger.LogInformation(
                    "Event received: {EventId} -> {DestinationUrl} (retry #{RetryCount})",
                    message.EventId, message.DestinationUrl, message.RetryCount);

                using var httpClient = new HttpClient();
                ApplyAuth(httpClient, message, decryption);

                var content = new StringContent(message.Payload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(message.DestinationUrl, content, stoppingToken);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    logger.LogWarning(
                        "Authentication failed ({StatusCode}) for event {EventId}. Removing from queue.",
                        (int)response.StatusCode, message.EventId);

                    await MarkAuthenticationFailedAsync(message.EventId, stoppingToken);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    return;
                }

                response.EnsureSuccessStatusCode();

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

    private void ApplyAuth(HttpClient client, EventMessage message, AesCredentialDecryptionService decryptionService)
    {
        if (message.AuthType is null || message.CredentialsEncrypted is null)
            return;

        var credentialsJson = decryptionService.Decrypt(message.CredentialsEncrypted);
        using var credentials = JsonDocument.Parse(credentialsJson);
        var root = credentials.RootElement;

        switch (message.AuthType)
        {
            case 0: // ApiKey
                var headerName = root.GetProperty("headerName").GetString()!;
                var key = root.GetProperty("key").GetString()!;
                client.DefaultRequestHeaders.Add(headerName, key);
                break;

            case 1: // BearerToken
                var token = root.GetProperty("token").GetString()!;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                break;

            case 2: // JwtBearer — token OAuth2 buscado de forma síncrona para simplificar
                var accessToken = FetchJwtToken(root);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
                break;

            case 3: // BasicAuth
                var username = root.GetProperty("username").GetString()!;
                var password = root.GetProperty("password").GetString()!;
                var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", encoded);
                break;
        }
    }

    private static string FetchJwtToken(JsonElement credentials)
    {
        var clientId = credentials.GetProperty("clientId").GetString()!;
        var clientSecret = credentials.GetProperty("clientSecret").GetString()!;
        var tokenUrl = credentials.GetProperty("tokenUrl").GetString()!;
        var scope = credentials.TryGetProperty("scope", out var s) ? s.GetString() : null;

        using var client = new HttpClient();
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
        };
        if (scope is not null) form["scope"] = scope;

        var response = client.PostAsync(tokenUrl, new FormUrlEncodedContent(form)).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        return json.RootElement.GetProperty("access_token").GetString()!;
    }

    private async Task MarkAuthenticationFailedAsync(Guid eventId, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand(
            "UPDATE eventos SET status = $1 WHERE id = $2");
        cmd.Parameters.AddWithValue(7); // AuthenticationFailed
        cmd.Parameters.AddWithValue(eventId);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
