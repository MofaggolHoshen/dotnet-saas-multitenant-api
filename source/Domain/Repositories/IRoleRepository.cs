using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(TenantId tenantId, string name, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    void Update(Role role);
    void Remove(Role role);
}
