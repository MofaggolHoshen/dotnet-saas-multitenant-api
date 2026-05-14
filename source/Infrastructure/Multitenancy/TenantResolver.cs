using Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Multitenancy;

/// <summary>
/// Defines the contract for resolving tenant information from HTTP request context.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Resolves the tenant from the given HTTP context using configured resolution strategies.
    /// </summary>
    /// <param name="context">The HTTP context containing request information.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple containing TenantId and TenantName if resolved; otherwise null.</returns>
    Task<(Guid TenantId, string TenantName)?> ResolveTenantAsync(HttpContext context, CancellationToken ct = default);
}

/// <summary>
/// Resolves tenant consistently from request context using multiple strategies.
/// Resolution order: Header (X-Tenant-Id) -> Subdomain -> Route parameter.
/// </summary>
public sealed class TenantResolver : ITenantResolver
{
    private readonly ITenantRepository _tenantRepository;

    public TenantResolver(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<(Guid TenantId, string TenantName)?> ResolveTenantAsync(HttpContext context, CancellationToken ct = default)
    {
        // Strategy 1: Header-based resolution (X-Tenant-Id)
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var header) && 
            Guid.TryParse(header, out var tenantId))
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
            if (tenant is not null)
            {
                return (tenant.Id, tenant.Name);
            }
        }

        // Strategy 2: Subdomain-based resolution
        var host = context.Request.Host.Host;
        var subdomain = host.Split('.').FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(subdomain) && 
            !string.Equals(subdomain, "api", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(subdomain, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            var tenant = await _tenantRepository.GetBySubdomainAsync(subdomain, ct);
            if (tenant is not null)
            {
                return (tenant.Id, tenant.Name);
            }
        }

        // Strategy 3: Route parameter-based resolution (tenantId in route)
        if (context.Request.RouteValues.TryGetValue("tenantId", out var routeValue) && 
            Guid.TryParse(routeValue?.ToString(), out tenantId))
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
            if (tenant is not null)
            {
                return (tenant.Id, tenant.Name);
            }
        }

        // No tenant resolved
        return null;
    }
}
