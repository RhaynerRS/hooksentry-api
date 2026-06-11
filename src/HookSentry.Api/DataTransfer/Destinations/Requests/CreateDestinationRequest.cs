namespace HookSentry.Api.DataTransfer.Destinations.Requests;

public record CreateDestinationRequest(string Url, int ServerRateLimit = 5);
