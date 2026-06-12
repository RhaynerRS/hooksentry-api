using RabbitMQ.Client;

namespace HookSentry.Api.Common.RabbitMq;

public sealed class RabbitMqConnection : IAsyncDisposable
{
    private IConnection? _connection;

    public async Task ConnectAsync(RabbitMqSettings settings, CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(ct);
    }

    public IConnection GetConnection()
    {
        if (_connection is null)
            throw new InvalidOperationException("Call ConnectAsync before GetConnection.");

        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
