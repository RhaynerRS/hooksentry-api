namespace HookSentry.Api.Common.Validation;

internal static class InputSanitizer
{
    private const int MaxRefreshTokenLength = 512;
    private const int MaxEmailLength = 255;
    private const int MaxNameLength = 255;

    public static bool HasControlChars(string value) =>
        value.Any(c => c is '\r' or '\n' or '\0');

    public static bool IsValidHttpHeaderName(string name) =>
        name.Length > 0 && name.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');

    public static bool IsValidHttpsUrl(string url, int maxLength, out string? error)
    {
        if (url.Length > maxLength)
        {
            error = $"URL não pode exceder {maxLength} caracteres.";
            return false;
        }
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            error = "A URL deve ser uma URL HTTPS válida.";
            return false;
        }
        error = null;
        return true;
    }

    public static string? ValidateEmail(string email)
    {
        if (email.Length > MaxEmailLength)
            return $"'email' não pode exceder {MaxEmailLength} caracteres.";
        if (HasControlChars(email))
            return "'email' não pode conter caracteres de controle (\\r, \\n, \\0).";
        return null;
    }

    public static string? ValidateName(string name)
    {
        if (name.Length > MaxNameLength)
            return $"'name' não pode exceder {MaxNameLength} caracteres.";
        if (HasControlChars(name))
            return "'name' não pode conter caracteres de controle (\\r, \\n, \\0).";
        return null;
    }

    public static string? ValidateToken(string token)
    {
        if (token.Length > MaxRefreshTokenLength)
            return $"Token não pode exceder {MaxRefreshTokenLength} caracteres.";
        if (HasControlChars(token))
            return "Token não pode conter caracteres de controle (\\r, \\n, \\0).";
        return null;
    }
}
