namespace Application.Common.Interfaces;

/// <summary>
/// Provides access to the resolved tenant context for the current request.
/// Implemented by infrastructure layer based on tenant resolution strategy (subdomain, header, etc.).
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the resolved tenant's unique identifier.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Gets the resolved tenant's name, or null if not available.
    /// </summary>
    string? TenantName { get; }

    /// <summary>
    /// Gets a value indicating whether the tenant context has been successfully resolved.
    /// </summary>
    bool IsResolved { get; }
}
