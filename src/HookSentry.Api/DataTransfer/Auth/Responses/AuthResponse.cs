namespace HookSentry.Api.DataTransfer.Auth.Responses;

public record AuthResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
