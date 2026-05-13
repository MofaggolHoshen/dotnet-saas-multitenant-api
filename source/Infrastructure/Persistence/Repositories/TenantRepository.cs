using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(ApplicationDbContext context) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        return context.TenantsSet.FirstOrDefaultAsync(x => x.Id == tenantId, ct);
    }

    public Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default)
    {
        var normalizedSubdomain = subdomain.Trim().ToLowerInvariant();

        return context.TenantsSet.FirstOrDefaultAsync(x => x.Subdomain == normalizedSubdomain, ct);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.TenantsSet
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        return context.TenantsSet.AddAsync(tenant, ct).AsTask();
    }

    public void Update(Tenant tenant)
    {
        context.TenantsSet.Update(tenant);
    }

    public void Remove(Tenant tenant)
    {
        context.TenantsSet.Remove(tenant);
    }
}
