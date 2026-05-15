namespace Application.Features.Users.DTOs;

/// <summary>
/// Data transfer object for role information in user context.
/// </summary>
public sealed record UserRoleDto(
    Guid Id,
    string Name,
    string? Description = null
);
