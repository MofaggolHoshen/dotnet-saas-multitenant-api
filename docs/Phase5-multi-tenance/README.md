# Multi-Tenancy Architecture

## Overview

This document describes the multi-tenancy implementation in the .NET SaaS Multi-Tenant API. The system uses a **shared database with tenant discriminator** approach, providing strong tenant isolation through query filters and middleware-based tenant resolution.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Tenant Resolution Strategies](#tenant-resolution-strategies)
3. [Tenant Context](#tenant-context)
4. [Query Filters](#query-filters)
5. [Tenant Middleware](#tenant-middleware)
6. [Tenant Provisioning](#tenant-provisioning)
7. [Bypassing Filters](#bypassing-filters)
8. [Testing Tenant Isolation](#testing-tenant-isolation)
9. [Migration to Database-Per-Tenant](#migration-to-database-per-tenant)
10. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

The multi-tenancy system consists of several key components:

```
┌─────────────────┐
│  HTTP Request   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ TenantMiddleware│  ← Resolves tenant from request
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  TenantContext  │  ← Scoped container for tenant state
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Query Filters   │  ← Automatic tenant filtering at DB level
└─────────────────┘
```

**Key Principles:**

- **Tenant Isolation**: Data is isolated using TenantId discriminator column
- **Automatic Filtering**: EF Core query filters enforce tenant boundaries
- **Request-Scoped Context**: Each request has its own tenant context
- **Multiple Resolution Strategies**: Support for header, subdomain, and route-based resolution

---

## Tenant Resolution Strategies

The `TenantResolver` supports multiple strategies to identify the tenant from an HTTP request. Strategies are tried in the following order:

### 1. Header-Based Resolution (Recommended for APIs)

The tenant is identified using the `X-Tenant-Id` header:

```http
GET /api/v1/users HTTP/1.1
Host: api.example.com
X-Tenant-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer <token>
```

**Pros:**

- Simple and explicit
- Works with any domain
- Easy to test

**Cons:**

- Clients must include the header
- Not user-friendly for browser-based apps

### 2. Subdomain-Based Resolution

The tenant is identified from the subdomain:

```http
GET /api/v1/users HTTP/1.1
Host: acme.api.example.com
Authorization: Bearer <token>
```

In this example, `acme` is the tenant subdomain.

**Pros:**

- User-friendly URLs
- Natural tenant separation
- Good for SaaS products

**Cons:**

- Requires DNS/routing configuration
- Subdomain must be unique per tenant
- Localhost testing requires hosts file or DNS override

**Excluded Subdomains:**

- `api` (reserved for API gateway)
- `localhost` (development)

### 3. Route Parameter-Based Resolution

The tenant is identified from the route:

```http
GET /api/v1/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/users HTTP/1.1
Host: api.example.com
Authorization: Bearer <token>
```

**Pros:**

- Works without custom headers or DNS
- Clear and explicit in URL

**Cons:**

- Longer URLs
- Tenant ID exposed in routes

---

## Tenant Context

### ITenantContext Interface

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantName { get; }
    bool IsResolved { get; }
}
```

### TenantContext Implementation

The `TenantContext` is a **scoped service** (one instance per HTTP request):

```csharp
public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantName { get; private set; }
    public bool IsResolved { get; private set; }

    public void SetTenant(Guid tenantId, string? tenantName) { ... }
    public void Clear() { ... }
}
```

**Lifecycle:**

1. Created when request starts
2. Populated by `TenantMiddleware`
3. Injected into handlers, repositories, DbContext
4. Disposed when request ends

**Usage in Code:**

```csharp
public class UserService
{
    private readonly ITenantContext _tenantContext;

    public UserService(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task DoSomething()
    {
        var tenantId = _tenantContext.TenantId;
        var tenantName = _tenantContext.TenantName;

        if (!_tenantContext.IsResolved)
        {
            throw new InvalidOperationException("Tenant not resolved");
        }
    }
}
```

---

## Query Filters

### What Are Query Filters?

Query filters are **global WHERE clauses** applied automatically to all queries for specific entities. They ensure tenant isolation at the database level.

### Implementation

In `ApplicationDbContext.OnModelCreating`:

```csharp
modelBuilder.Entity<User>()
    .HasQueryFilter(x =>
        !x.IsDeleted &&
        (!_tenantContext.IsResolved || x.TenantId.Value == _tenantContext.TenantId));

modelBuilder.Entity<Role>()
    .HasQueryFilter(x =>
        !x.IsDeleted &&
        (!_tenantContext.IsResolved || x.TenantId.Value == _tenantContext.TenantId || x.IsSystemRole));

modelBuilder.Entity<Tenant>()
    .HasQueryFilter(x => !x.IsDeleted);

modelBuilder.Entity<Permission>()
    .HasQueryFilter(x => !x.IsDeleted);
```

### How It Works

**Without Query Filters:**

```csharp
var users = await context.Users.ToListAsync();
// SQL: SELECT * FROM Users
// Returns users from ALL tenants ❌
```

**With Query Filters:**

```csharp
var users = await context.Users.ToListAsync();
// SQL: SELECT * FROM Users WHERE IsDeleted = 0 AND TenantId = 'xxx'
// Returns users only from current tenant ✅
```

### Special Cases

- **Tenant entity**: No tenant filter (tenants can query themselves)
- **Permission entity**: No tenant filter (permissions are global)
- **System Roles**: Included even if from different tenant (via `|| x.IsSystemRole`)

---

## Tenant Middleware

### Purpose

The `TenantMiddleware` resolves and sets the tenant context early in the request pipeline, before any business logic executes.

### Pipeline Position

```csharp
app.UseHttpsRedirection();
app.UseRouting();          // Must be before tenant middleware

app.UseTenantResolution(); // ← Tenant middleware here

app.UseAuthorization();    // After tenant resolution
app.MapControllers();
```

**Important:** Must be placed **after** `UseRouting()` and **before** `UseAuthorization()`.

### Bypass Logic

Certain endpoints don't require tenant resolution:

```csharp
private static readonly string[] BypassPrefixes =
[
    "/health",              // Health checks
    "/swagger",             // Swagger UI
    "/api/v1/auth",         // Authentication endpoints
    "/_health",             // Internal health
    "/_configuration"       // Configuration endpoints
];
```

**Why Bypass?**

- **Health checks**: Infrastructure monitoring
- **Swagger**: API documentation
- **Auth endpoints**: Login, register (before tenant known)

### Error Handling

If tenant cannot be resolved:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "Tenant not specified or invalid."
}
```

---

## Tenant Provisioning

### Creating a New Tenant

The `TenantService` handles tenant creation:

```csharp
var result = await tenantService.CreateTenantAsync(
    name: "Acme Corporation",
    subdomain: "acme",
    tier: "Pro",
    cancellationToken);

if (result.IsSuccess)
{
    var tenant = result.Value;
    Console.WriteLine($"Tenant created: {tenant.Id}");
}
```

### Default Data Seeding

When a tenant is created, the `TenantProvisionedEvent` is raised and handled by `TenantProvisionedEventHandler`, which seeds:

**Default Roles:**

- **Admin**: Full permissions (users._, roles._, tenant.\*)
- **User**: Limited permissions (users.read, tenant.read)
- **Viewer**: Read-only permissions (users.read, tenant.read)

**Default Permissions:**

- `users.read`, `users.write`, `users.delete`
- `roles.read`, `roles.write`, `roles.delete`
- `tenant.read`, `tenant.write`

### Provisioning Flow

```
1. TenantService.CreateTenantAsync()
   ├─ Validate subscription tier
   ├─ Create Tenant entity
   ├─ Check subdomain uniqueness
   ├─ Persist to database
   └─ Raise TenantProvisionedEvent

2. TenantProvisionedEventHandler.Handle()
   ├─ Seed Admin role
   ├─ Seed User role
   ├─ Seed Viewer role
   └─ Persist roles to database
```

---

## Bypassing Filters

### When to Bypass

Bypassing query filters is necessary for:

- **Super Admin Operations**: Cross-tenant reporting, analytics
- **System Maintenance**: Data migrations, cleanup scripts
- **Auditing**: Compliance reports across all tenants

### How to Bypass

Use the `QueryIgnoringFilters<T>()` method:

```csharp
// WITH filters (normal usage)
var users = await context.Users.ToListAsync();
// Returns only current tenant's users

// WITHOUT filters (admin/reporting)
var allUsers = await context.QueryIgnoringFilters<User>().ToListAsync();
// Returns users from ALL tenants
```

### ⚠️ Security Warning

**Always validate permissions before bypassing filters:**

```csharp
if (!currentUser.IsSuperAdmin)
{
    throw new ForbiddenException("Only super admins can access cross-tenant data");
}

var allTenants = await context.QueryIgnoringFilters<Tenant>()
    .Where(t => t.IsActive)
    .ToListAsync();
```

---

## Testing Tenant Isolation

### Unit Tests

Test tenant resolution:

```csharp
[Fact]
public async Task TenantResolver_HeaderBased_ResolvesTenant()
{
    // Arrange
    var tenantId = Guid.NewGuid();
    var httpContext = CreateHttpContext();
    httpContext.Request.Headers["X-Tenant-Id"] = tenantId.ToString();

    // Act
    var result = await resolver.ResolveTenantAsync(httpContext);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(tenantId, result.Value.TenantId);
}
```

### Integration Tests

Test tenant isolation:

```csharp
[Fact]
public async Task Users_Query_ReturnsOnlyCurrentTenantUsers()
{
    // Arrange: Create users for Tenant A and Tenant B
    var tenantA = await CreateTenantAsync("Tenant A", "tenant-a");
    var tenantB = await CreateTenantAsync("Tenant B", "tenant-b");

    var userA = await CreateUserAsync(tenantA.Id, "user@tenanta.com");
    var userB = await CreateUserAsync(tenantB.Id, "user@tenantb.com");

    // Act: Query as Tenant A
    SetTenantContext(tenantA.Id);
    var users = await context.Users.ToListAsync();

    // Assert: Should only see Tenant A's users
    Assert.Single(users);
    Assert.Equal(userA.Id, users[0].Id);
}
```

---

## Migration to Database-Per-Tenant

### Current: Shared Database with Discriminator

All tenants share one database with a `TenantId` column.

**Pros:**

- Simpler deployment
- Lower infrastructure cost
- Easier backups and maintenance

**Cons:**

- Scaling limits (single database)
- Risk of noisy neighbor issues
- Harder to provide tenant-specific SLAs

### Future: Database-Per-Tenant

Each tenant gets its own database.

**Migration Steps:**

1. **Update Connection String Strategy:**

   ```csharp
   public class TenantConnectionStringResolver
   {
       public string GetConnectionString(Guid tenantId)
       {
           return $"Host=localhost;Database=Tenant_{tenantId};...";
       }
   }
   ```

2. **Update DbContext Registration:**

   ```csharp
   services.AddDbContext<ApplicationDbContext>((sp, options) =>
   {
       var tenantContext = sp.GetRequiredService<ITenantContext>();
       var connectionString = resolver.GetConnectionString(tenantContext.TenantId);
       options.UseNpgsql(connectionString);
   });
   ```

3. **Remove Query Filters:**
   - No longer needed since each tenant has isolated database

4. **Update Migrations:**
   - Run migrations per-tenant database

---

## Troubleshooting

### Issue: "Tenant not specified or invalid"

**Cause:** Tenant could not be resolved from the request.

**Solutions:**

- Ensure `X-Tenant-Id` header is set
- Verify subdomain is correct
- Check tenant exists in database
- Verify endpoint is not in bypass list

### Issue: Cross-tenant data leakage

**Cause:** Query filters not working or bypassed incorrectly.

**Solutions:**

- Verify `TenantContext.IsResolved` is true
- Check query filter configuration in `ApplicationDbContext`
- Ensure `IgnoreQueryFilters()` is not used accidentally
- Review integration tests

### Issue: Query filter not applied

**Cause:** DbContext initialized before tenant context set.

**Solutions:**

- Ensure `TenantMiddleware` runs before controllers
- Verify `TenantContext` is scoped (not singleton)
- Check middleware order in `Program.cs`

### Issue: Domain events not firing

**Cause:** `IPublisher` is null or not injected.

**Solutions:**

- Verify `AddMediatR` is called in `Application.DependencyInjection`
- Check `ApplicationDbContext` receives `IPublisher` parameter
- Ensure `SaveChangesAsync` override calls `DispatchDomainEventsAsync`

---

## Summary

The multi-tenancy implementation provides:

✅ **Strong Isolation**: Query filters enforce tenant boundaries at DB level  
✅ **Flexible Resolution**: Support for header, subdomain, and route strategies  
✅ **Automatic Provisioning**: New tenants get default roles and permissions  
✅ **Scalable Architecture**: Easy migration to database-per-tenant in future  
✅ **Secure by Default**: Middleware ensures tenant context is always set

For questions or issues, refer to the [Troubleshooting](#troubleshooting) section or consult the development team.
