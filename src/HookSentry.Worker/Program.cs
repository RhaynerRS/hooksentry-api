using HookSentry.Worker.Consumers;
using HookSentry.Worker.Infrastructure.RabbitMq;
using HookSentry.Worker.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.Configure<CredentialEncryptionSettings>(
    builder.Configuration.GetSection("CredentialEncryption"));

var dataSource = NpgsqlDataSource.Create(
    builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddSingleton(dataSource);

builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<AesCredentialDecryptionService>();

builder.Services.AddHostedService<WebhookDeliveryConsumer>();

var host = builder.Build();

var mqConnection = host.Services.GetRequiredService<RabbitMqConnection>();
var settings = host.Services.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
await mqConnection.ConnectAsync(settings, CancellationToken.None);

await host.RunAsync();
