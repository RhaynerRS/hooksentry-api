namespace HookSentry.Api.Common.DTOs;

public record PaginationResponse<T>(int Total, IReadOnlyList<T> Items);
