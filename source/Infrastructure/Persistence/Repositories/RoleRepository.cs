using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        return context.RolesSet.FirstOrDefaultAsync(x => x.Id == roleId, ct);
    }

    public Task<Role?> GetByNameAsync(TenantId tenantId, string name, CancellationToken ct = default)
    {
        var normalizedName = name.Trim();

        return context.RolesSet.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Name == normalizedName,
            ct);
    }

    public async Task<IReadOnlyList<Role>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default)
    {
        return await context.RolesSet
            .Where(x => x.TenantId == tenantId || x.IsSystemRole)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken ct = default)
    {
        var ids = roleIds.Distinct().ToArray();

        return await context.RolesSet
            .Where(x => ids.Contains(x.Id))
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public Task AddAsync(Role role, CancellationToken ct = default)
    {
        return context.RolesSet.AddAsync(role, ct).AsTask();
    }

    public void Update(Role role)
    {
        context.RolesSet.Update(role);
    }

    public void Remove(Role role)
    {
        context.RolesSet.Remove(role);
    }
}
