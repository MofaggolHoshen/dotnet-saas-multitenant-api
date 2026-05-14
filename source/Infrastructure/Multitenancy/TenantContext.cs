using Application.Common.Interfaces;

namespace Infrastructure.Multitenancy;

/// <summary>
/// Scoped tenant state container per request.
/// Provides mutable tenant context that can be set by middleware during request processing.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantName { get; private set; }
    public bool IsResolved { get; private set; }

    /// <summary>
    /// Sets the tenant context for the current request.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <param name="tenantName">The tenant's name (optional).</param>
    public void SetTenant(Guid tenantId, string? tenantName)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        IsResolved = tenantId != Guid.Empty;
    }

    /// <summary>
    /// Clears the tenant context.
    /// </summary>
    public void Clear()
    {
        TenantId = Guid.Empty;
        TenantName = null;
        IsResolved = false;
    }
}
