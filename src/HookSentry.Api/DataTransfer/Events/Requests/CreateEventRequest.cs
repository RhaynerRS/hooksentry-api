using System.Text.Json;

namespace HookSentry.Api.DataTransfer.Events.Requests;

public record CreateEventRequest(Guid DestinationUrlId, JsonElement Payload);
