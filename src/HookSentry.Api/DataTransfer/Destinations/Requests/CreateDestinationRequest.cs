using System.Text.Json;

namespace HookSentry.Api.DataTransfer.Destinations.Requests;

public record CreateDestinationRequest(
    string Url,
    int ServerRateLimit = 5,
    string? AuthType = null,
    JsonElement? Credentials = null);
