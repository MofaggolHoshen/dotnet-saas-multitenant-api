namespace Application.Features.Users.DTOs;

/// <summary>
/// Data transfer object for detailed user information.
/// </summary>
public sealed record UserDetailDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    Guid TenantId,
    string? TenantName,
    List<UserRoleDto> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt = null,
    DateTime? LastLoginAt = null
);
