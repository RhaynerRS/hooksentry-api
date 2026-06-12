namespace HookSentry.Infrastructure.RabbitMq;

public sealed class RabbitMqSettings
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string EventsExchange { get; init; } = "hooksentry.events";
}
