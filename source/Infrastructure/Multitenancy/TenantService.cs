using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;

namespace Infrastructure.Multitenancy;

/// <summary>
/// Defines the contract for tenant management operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Creates and provisions a new tenant with default data.
    /// </summary>
    /// <param name="name">The tenant's display name.</param>
    /// <param name="subdomain">The tenant's subdomain identifier.</param>
    /// <param name="tier">The subscription tier (Free, Pro, Enterprise).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the created Tenant or an error.</returns>
    Task<Result<Tenant>> CreateTenantAsync(string name, string subdomain, string tier, CancellationToken ct = default);

    /// <summary>
    /// Validates that a tenant exists and is active.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if tenant exists and is active; otherwise false.</returns>
    Task<bool> ValidateTenantAsync(Guid tenantId, CancellationToken ct = default);
}

/// <summary>
/// Handles tenant creation and provisioning workflow.
/// </summary>
public sealed class TenantService : ITenantService
{
    private readonly ITenantRepository _tenants;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(
        ITenantRepository tenants, 
        IRoleRepository roles, 
        IUnitOfWork unitOfWork)
    {
        _tenants = tenants;
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Tenant>> CreateTenantAsync(
        string name, 
        string subdomain, 
        string tier, 
        CancellationToken ct = default)
    {
        // Validate subscription tier
        var tierResult = SubscriptionTier.Create(tier);
        if (tierResult.IsFailure)
        {
            return Result<Tenant>.Failure(tierResult.Error);
        }

        // Create tenant entity
        var tenantResult = Tenant.Create(name, subdomain, tierResult.Value);
        if (tenantResult.IsFailure)
        {
            return tenantResult;
        }

        var tenant = tenantResult.Value;

        // Check if subdomain already exists
        var existingTenant = await _tenants.GetBySubdomainAsync(subdomain, ct);
        if (existingTenant is not null)
        {
            return Result<Tenant>.Failure(Error.Conflict($"Subdomain '{subdomain}' is already taken."));
        }

        // Persist tenant
        await _tenants.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Note: Tenant-specific default data (roles, permissions) should be seeded
        // via domain event handlers (TenantProvisionedEventHandler)
        // to maintain separation of concerns

        return Result<Tenant>.Success(tenant);
    }

    public async Task<bool> ValidateTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenants.GetByIdAsync(tenantId, ct);
        return tenant is not null && tenant.IsActive;
    }
}
