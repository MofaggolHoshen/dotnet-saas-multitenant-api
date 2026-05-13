using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return context.UsersSet.FirstOrDefaultAsync(x => x.Id == userId, ct);
    }

    public Task<User?> GetByEmailAsync(TenantId tenantId, string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return context.UsersSet.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Email.Value == normalizedEmail,
            ct);
    }

    public async Task<IReadOnlyList<User>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default)
    {
        return await context.UsersSet
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        return context.UsersSet.AddAsync(user, ct).AsTask();
    }

    public void Update(User user)
    {
        context.UsersSet.Update(user);
    }

    public void Remove(User user)
    {
        context.UsersSet.Remove(user);
    }
}
