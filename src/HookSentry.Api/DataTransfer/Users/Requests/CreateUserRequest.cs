using HookSentry.Api.Features.Users.Domain;

namespace HookSentry.Api.DataTransfer.Users.Requests;

public record CreateUserRequest(
    string Email,
    string Password,
    UserRole Role = UserRole.Developer);
