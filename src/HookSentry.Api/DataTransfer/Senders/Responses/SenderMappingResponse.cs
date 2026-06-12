using System.Text.Json;

namespace HookSentry.Api.DataTransfer.Senders.Responses;

public record SenderMappingResponse(JsonElement Mapping);
