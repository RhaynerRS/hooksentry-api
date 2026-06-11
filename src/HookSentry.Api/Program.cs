using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpoints()
    .AddPersistence(builder.Configuration)
    .AddSecurity()
    .AddJwtAndApiKeyAuth(builder.Configuration)
    .AddSwaggerWithAuth();

var app = builder.Build();

app.UseSwaggerWithAuth();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

app.Run();
