using HookSentry.Api.Features.Users.Domain;

namespace HookSentry.Api.DataTransfer.Users.Responses;

public record UserResponse(
    Guid Id,
    Guid TenantId,
    string Email,
    UserStatus Status,
    UserRole Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static UserResponse From(User u) =>
        new(u.Id, u.TenantId, u.Email, u.Status, u.Role, u.CreatedAt, u.UpdatedAt);
}
