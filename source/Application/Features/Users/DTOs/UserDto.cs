namespace Application.Features.Users.DTOs;

/// <summary>
/// Data transfer object for user information in list views.
/// </summary>
public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt = null
);
