namespace HookSentry.Api.DataTransfer.Events.Responses;

public record EventAcceptedResponse(Guid EventId, string Status, DateTimeOffset AcceptedAt);
