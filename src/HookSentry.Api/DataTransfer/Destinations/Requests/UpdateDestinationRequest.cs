using System.Text.Json;

namespace HookSentry.Api.DataTransfer.Destinations.Requests;

public record UpdateDestinationRequest(
    string? Url = null,
    int? ServerRateLimit = null,
    string? Status = null,
    string? AuthType = null,
    JsonElement? Credentials = null,
    bool? RemoveAuth = null);
