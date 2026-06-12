using System.Security.Claims;
using System.Text.Json;
using HookSentry.Api.Common.Endpoints;
using HookSentry.Api.Common.Extensions;
using HookSentry.Infrastructure.Security;
using HookSentry.Api.DataTransfer.Destinations.Requests;
using HookSentry.Api.DataTransfer.Destinations.Responses;
using HookSentry.Domain.Destinations;
using HookSentry.Domain.Tenants;

namespace HookSentry.Api.Features.Destinations.CreateDestination;

public class CreateDestinationEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/destinations", Handle)
            .WithName("CreateDestination")
            .WithTags("Destinations")
            .WithSummary("Cadastra uma nova URL de destino para o tenant autenticado")
            .WithDescription("""
                Registra uma URL HTTPS de destino para receber webhooks entregues pelo HookSentry.

                **Body:**
                - `url` *(obrigatório)*: URL HTTPS válida do endpoint de destino
                - `serverRateLimit` *(opcional, padrão: 5)*: número máximo de requisições simultâneas (RF-006)
                - `authType` *(opcional)*: tipo de autenticação — `ApiKey`, `BearerToken`, `JwtBearer`, `BasicAuth`
                - `credentials` *(obrigatório se authType informado)*: objeto JSON com as credenciais (RF-019)

                **Estrutura de `credentials` por tipo:**
                - `ApiKey`: `{ "key": "...", "headerName": "X-Api-Key" }`
                - `BearerToken`: `{ "token": "..." }`
                - `JwtBearer`: `{ "clientId": "...", "clientSecret": "...", "tokenUrl": "...", "scope": "..." }`
                - `BasicAuth`: `{ "username": "...", "password": "..." }`

                As credenciais são criptografadas com AES-256-GCM antes de persistir. Nunca são retornadas em respostas.

                **Códigos de retorno:**
                - `201 Created`: URL de destino criada
                - `400 Bad Request`: URL inválida, credenciais malformadas ou authType desconhecido
                - `401 Unauthorized`: token ausente ou inválido
                - `404 Not Found`: tenant não encontrado
                """)
            .RequireAuthorization()
            .Produces<DestinationResponse>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handle(
        CreateDestinationRequest request,
        ClaimsPrincipal user,
        NHibernate.ISession session,
        ICredentialEncryptionService encryption,
        CancellationToken ct)
    {
        if (user.RequireTenantId(out var tenantId) is { } err) return err;

        var tenant = await session.GetAsync<Tenant>(tenantId, ct);
        if (tenant is null)
            return Results.NotFound($"Tenant '{tenantId}' not found.");

        DestinationAuthType? authType = null;
        string? credentialsEncrypted = null;

        if (request.AuthType is not null || request.Credentials.HasValue)
        {
            if (request.AuthType is null)
                return Results.BadRequest("'authType' é obrigatório quando 'credentials' é informado.");

            if (!request.Credentials.HasValue)
                return Results.BadRequest("'credentials' é obrigatório quando 'authType' é informado.");

            if (!Enum.TryParse<DestinationAuthType>(request.AuthType, ignoreCase: true, out var parsedType))
                return Results.BadRequest(
                    $"AuthType '{request.AuthType}' inválido. Valores aceitos: ApiKey, BearerToken, JwtBearer, BasicAuth.");

            var validationError = ValidateCredentials(parsedType, request.Credentials.Value);
            if (validationError is not null)
                return Results.BadRequest(validationError);

            authType = parsedType;
            credentialsEncrypted = encryption.Encrypt(request.Credentials.Value.GetRawText());
        }

        DestinationUrl destination;
        try
        {
            destination = new DestinationUrl(tenantId, request.Url, request.ServerRateLimit);
            destination.SetAuth(authType, credentialsEncrypted);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }

        using var tx = session.BeginTransaction();
        await session.SaveAsync(destination, ct);
        await tx.CommitAsync(ct);

        return Results.Created(
            $"/api/v1/destinations/{destination.Id}",
            DestinationResponse.From(destination));
    }

    private static string? ValidateCredentials(DestinationAuthType authType, JsonElement credentials)
    {
        return authType switch
        {
            DestinationAuthType.ApiKey =>
                RequireStrings(credentials, "key", "headerName"),
            DestinationAuthType.BearerToken =>
                RequireStrings(credentials, "token"),
            DestinationAuthType.JwtBearer =>
                RequireStrings(credentials, "clientId", "clientSecret", "tokenUrl"),
            DestinationAuthType.BasicAuth =>
                RequireStrings(credentials, "username", "password"),
            _ => $"AuthType '{authType}' não suportado."
        };
    }

    private static string? RequireStrings(JsonElement json, params string[] fields)
    {
        foreach (var field in fields)
        {
            if (!json.TryGetProperty(field, out var prop) ||
                prop.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(prop.GetString()))
                return $"Campo '{field}' é obrigatório e não pode ser vazio para o tipo de autenticação informado.";
        }
        return null;
    }
}
