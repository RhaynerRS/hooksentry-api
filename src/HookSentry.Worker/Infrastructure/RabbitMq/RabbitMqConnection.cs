using RabbitMQ.Client;

namespace HookSentry.Worker.Infrastructure.RabbitMq;

// Responsável por criar e expor uma IConnection reutilizável para o processo inteiro.
// Um único IConnection pode abrir múltiplos IChannel (um por consumer/thread).
public sealed class RabbitMqConnection : IAsyncDisposable
{
    private IConnection? _connection;

    public async Task ConnectAsync(RabbitMqSettings settings, CancellationToken ct)
    {
        ConnectionFactory factory = new()
        {
            UserName = settings.Username,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            HostName = settings.Host,
            Port = settings.Port
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
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}
