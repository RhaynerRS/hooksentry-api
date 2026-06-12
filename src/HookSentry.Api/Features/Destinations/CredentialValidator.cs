using System.Text.Json;
using HookSentry.Domain.Destinations;

namespace HookSentry.Api.Features.Destinations;

internal static class CredentialValidator
{
    private const int MaxHeaderNameLength = 100;
    private const int MaxHeaderValueLength = 2048;
    private const int MaxTokenLength = 4096;
    private const int MaxIdentifierLength = 512;
    private const int MaxUrlLength = 2048;
    private const int MaxScopeLength = 512;

    public static string? Validate(DestinationAuthType authType, JsonElement credentials) =>
        authType switch
        {
            DestinationAuthType.ApiKey      => ValidateApiKey(credentials),
            DestinationAuthType.BearerToken => ValidateBearerToken(credentials),
            DestinationAuthType.JwtBearer   => ValidateJwtBearer(credentials),
            DestinationAuthType.BasicAuth   => ValidateBasicAuth(credentials),
            _ => $"AuthType '{authType}' não suportado."
        };

    private static string? ValidateApiKey(JsonElement json)
    {
        var err = RequireStrings(json, "headerName", "value");
        if (err is not null) return err;

        var headerName = json.GetProperty("headerName").GetString()!;
        if (headerName.Length > MaxHeaderNameLength)
            return $"'headerName' não pode exceder {MaxHeaderNameLength} caracteres.";
        if (!IsValidHttpHeaderName(headerName))
            return "'headerName' contém caracteres inválidos. Use apenas letras, dígitos e hífens.";

        var value = json.GetProperty("value").GetString()!;
        if (value.Length > MaxHeaderValueLength)
            return $"'value' não pode exceder {MaxHeaderValueLength} caracteres.";
        if (HasControlChars(value))
            return "'value' não pode conter caracteres de controle (\\r, \\n, \\0).";

        return null;
    }

    private static string? ValidateBearerToken(JsonElement json)
    {
        var err = RequireStrings(json, "token");
        if (err is not null) return err;

        var token = json.GetProperty("token").GetString()!;
        if (token.Length > MaxTokenLength)
            return $"'token' não pode exceder {MaxTokenLength} caracteres.";
        if (HasControlChars(token))
            return "'token' não pode conter caracteres de controle (\\r, \\n, \\0).";

        return null;
    }

    private static string? ValidateJwtBearer(JsonElement json)
    {
        var err = RequireStrings(json, "tokenEndpoint", "clientId", "clientSecret");
        if (err is not null) return err;

        var tokenEndpoint = json.GetProperty("tokenEndpoint").GetString()!;
        if (!IsValidHttpsUrl(tokenEndpoint, MaxUrlLength, out var urlErr))
            return urlErr;

        var clientId = json.GetProperty("clientId").GetString()!;
        if (clientId.Length > MaxIdentifierLength)
            return $"'clientId' não pode exceder {MaxIdentifierLength} caracteres.";
        if (HasControlChars(clientId))
            return "'clientId' não pode conter caracteres de controle.";

        var clientSecret = json.GetProperty("clientSecret").GetString()!;
        if (clientSecret.Length > MaxIdentifierLength)
            return $"'clientSecret' não pode exceder {MaxIdentifierLength} caracteres.";
        if (HasControlChars(clientSecret))
            return "'clientSecret' não pode conter caracteres de controle.";

        if (json.TryGetProperty("scope", out var scopeProp) &&
            scopeProp.ValueKind == JsonValueKind.String)
        {
            var scope = scopeProp.GetString()!;
            if (scope.Length > MaxScopeLength)
                return $"'scope' não pode exceder {MaxScopeLength} caracteres.";
            if (HasControlChars(scope))
                return "'scope' não pode conter caracteres de controle.";
        }

        return null;
    }

    private static string? ValidateBasicAuth(JsonElement json)
    {
        var err = RequireStrings(json, "username", "password");
        if (err is not null) return err;

        var username = json.GetProperty("username").GetString()!;
        if (username.Length > MaxIdentifierLength)
            return $"'username' não pode exceder {MaxIdentifierLength} caracteres.";
        if (HasControlChars(username))
            return "'username' não pode conter caracteres de controle.";
        if (username.Contains(':'))
            return "'username' não pode conter ':' (RFC 7617 — Basic Auth não suporta dois-pontos no username).";

        var password = json.GetProperty("password").GetString()!;
        if (password.Length > MaxIdentifierLength)
            return $"'password' não pode exceder {MaxIdentifierLength} caracteres.";
        if (HasControlChars(password))
            return "'password' não pode conter caracteres de controle.";

        return null;
    }

    private static string? RequireStrings(JsonElement json, params string[] fields)
    {
        foreach (var field in fields)
        {
            if (!json.TryGetProperty(field, out var prop) ||
                prop.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(prop.GetString()))
                return $"Campo '{field}' é obrigatório e não pode ser vazio.";
        }
        return null;
    }

    private static bool HasControlChars(string value) =>
        value.Any(c => c is '\r' or '\n' or '\0');

    private static bool IsValidHttpHeaderName(string name) =>
        name.Length > 0 && name.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');

    private static bool IsValidHttpsUrl(string url, int maxLength, out string? error)
    {
        if (url.Length > maxLength)
        {
            error = $"'tokenEndpoint' não pode exceder {maxLength} caracteres.";
            return false;
        }
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            error = "'tokenEndpoint' deve ser uma URL HTTPS válida (SSRF: URLs não-HTTPS não são aceitas).";
            return false;
        }
        error = null;
        return true;
    }
}
