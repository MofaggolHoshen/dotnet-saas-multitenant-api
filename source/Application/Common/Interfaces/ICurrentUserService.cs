namespace Application.Common.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's information.
/// Implemented by infrastructure layer to extract claims from HTTP context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's unique identifier, or null if not authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's email address, or null if not authenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's tenant identifier, or null if not authenticated.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
