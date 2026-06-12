using HookSentry.Worker.Consumers;
using HookSentry.Worker.Infrastructure.RabbitMq;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

// RabbitMqConnection é singleton: uma conexão TCP para todo o processo.
builder.Services.AddSingleton<RabbitMqConnection>();

builder.Services.AddHostedService<WebhookDeliveryConsumer>();

var host = builder.Build();

// Abre a conexão com o broker antes de iniciar os consumers.
var mqConnection = host.Services.GetRequiredService<RabbitMqConnection>();
var settings = host.Services.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
await mqConnection.ConnectAsync(settings, CancellationToken.None);

await host.RunAsync();
