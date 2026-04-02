using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(TenantId tenantId, string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    void Remove(User user);
}
