using System.Reflection;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : this(options, UnresolvedTenantContext.Instance)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<User> UsersSet => Set<User>();
    public DbSet<Tenant> TenantsSet => Set<Tenant>();
    public DbSet<Role> RolesSet => Set<Role>();
    public DbSet<Permission> PermissionsSet => Set<Permission>();

    IQueryable<User> IApplicationDbContext.Users => UsersSet;
    IQueryable<Tenant> IApplicationDbContext.Tenants => TenantsSet;
    IQueryable<Role> IApplicationDbContext.Roles => RolesSet;
    IQueryable<Permission> IApplicationDbContext.Permissions => PermissionsSet;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<User>()
            .HasQueryFilter(x => !x.IsDeleted && (!_tenantContext.IsResolved || x.TenantId.Value == _tenantContext.TenantId));

        modelBuilder.Entity<Role>()
            .HasQueryFilter(x => !x.IsDeleted && (!_tenantContext.IsResolved || x.TenantId.Value == _tenantContext.TenantId || x.IsSystemRole));

        modelBuilder.Entity<Tenant>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<Permission>()
            .HasQueryFilter(x => !x.IsDeleted);
    }

    public IQueryable<T> QueryIgnoringFilters<T>() where T : BaseEntity
    {
        return Set<T>().IgnoreQueryFilters();
    }

    private sealed class UnresolvedTenantContext : ITenantContext
    {
        public static readonly UnresolvedTenantContext Instance = new();

        public Guid TenantId => Guid.Empty;
        public string? TenantName => null;
        public bool IsResolved => false;
    }
}
