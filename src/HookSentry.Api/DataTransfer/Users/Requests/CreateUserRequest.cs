using HookSentry.Domain.Users;

namespace HookSentry.Api.DataTransfer.Users.Requests;

public record CreateUserRequest(
    string Email,
    string Password,
    UserRole Role = UserRole.Developer);
