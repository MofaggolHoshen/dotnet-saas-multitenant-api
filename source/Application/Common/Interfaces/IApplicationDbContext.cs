using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction for the application database context.
/// Keeps the Application layer independent of EF Core implementation details.
/// </summary>
public interface IApplicationDbContext
{
    // Note: Using IQueryable instead of DbSet to avoid EF Core dependency in Application layer
    // Infrastructure layer will implement this with DbContext.Set<T>()

    IQueryable<User> Users { get; }
    IQueryable<Tenant> Tenants { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<Permission> Permissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
