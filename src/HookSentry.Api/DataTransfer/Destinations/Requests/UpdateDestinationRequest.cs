namespace HookSentry.Api.DataTransfer.Destinations.Requests;

public record UpdateDestinationRequest(
    string? Url = null,
    int? ServerRateLimit = null,
    string? Status = null);
