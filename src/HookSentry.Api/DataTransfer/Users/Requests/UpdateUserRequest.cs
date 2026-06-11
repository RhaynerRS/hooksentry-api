using HookSentry.Api.Features.Users.Domain;

namespace HookSentry.Api.DataTransfer.Users.Requests;

public record UpdateUserRequest(
    string? Email = null,
    string? Password = null,
    UserRole? Role = null,
    UserStatus? Status = null);
