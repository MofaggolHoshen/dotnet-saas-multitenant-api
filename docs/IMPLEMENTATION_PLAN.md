# Implementation Plan - .NET SaaS Multi-Tenant API

**Project**: dotnet-saas-multitenant-api  
**Architecture**: Clean Architecture + CQRS + DDD  
**Created**: February 18, 2026  
**Last Updated**: February 18, 2026  
**Status**: 🟡 In Planning

---

## Quick Reference

| Phase | Name | Duration | Status | Priority |
|-------|------|----------|--------|----------|
| 1 | Solution Setup & Domain Foundation | 3 days | ⚪ Not Started | 🔴 Critical |
| 2 | Domain Entities & Value Objects | 4 days | ⚪ Not Started | 🔴 Critical |
| 3 | Application Layer Setup (MediatR) | 3 days | ⚪ Not Started | 🔴 Critical |
| 4 | Infrastructure - EF Core Setup | 4 days | ⚪ Not Started | 🔴 Critical |
| 5 | Multi-Tenancy Infrastructure | 5 days | ⚪ Not Started | 🔴 Critical |
| 6 | Authentication & JWT | 5 days | ⚪ Not Started | 🔴 Critical |
| 7 | Auth Feature - CQRS Implementation | 4 days | ⚪ Not Started | 🟠 High |
| 8 | Users Feature - CQRS Implementation | 5 days | ⚪ Not Started | 🟠 High |
| 9 | API Layer & Controllers | 4 days | ⚪ Not Started | 🟠 High |
| 10 | Tenants Feature - CQRS Implementation | 5 days | ⚪ Not Started | 🟠 High |
| 11 | Roles & Permissions Feature | 5 days | ⚪ Not Started | 🟡 Medium |
| 12 | MediatR Pipeline Behaviors | 3 days | ⚪ Not Started | 🟡 Medium |
| 13 | API Middleware & Error Handling | 3 days | ⚪ Not Started | 🟡 Medium |
| 14 | Swagger/OpenAPI Configuration | 2 days | ⚪ Not Started | 🟡 Medium |
| 15 | Docker Setup & Orchestration | 4 days | ⚪ Not Started | 🟡 Medium |
| 16 | Redis Caching Implementation | 3 days | ⚪ Not Started | 🟢 Low |
| 17 | Unit Tests - Domain & Application | 5 days | ⚪ Not Started | 🟠 High |
| 18 | Integration Tests - API & Database | 5 days | ⚪ Not Started | 🟠 High |
| 19 | Documentation & API Examples | 3 days | ⚪ Not Started | 🟡 Medium |
| 20 | Performance Optimization & Security | 4 days | ⚪ Not Started | 🟠 High |

**Total Estimated Duration**: ~76 days (~15 weeks)

---

## Status Legend

- ⚪ **Not Started** - Phase not begun
- 🔵 **In Progress** - Currently working on this phase
- 🟢 **Completed** - Phase finished and tested
- 🟡 **Blocked** - Waiting on dependencies or resources
- 🔴 **Issues** - Problems encountered, needs attention

## Priority Legend

- 🔴 **Critical** - Must be completed first, blocks other phases
- 🟠 **High** - Important for core functionality
- 🟡 **Medium** - Enhances functionality
- 🟢 **Low** - Nice to have, can be deferred

---

# Detailed Implementation Phases

## Phase 1: Solution Setup & Domain Foundation

**Duration**: 3 days  
**Status**: ⚪ Not Started  
**Priority**: 🔴 Critical  
**Dependencies**: None

### Objectives
- Create solution structure following Clean Architecture
- Set up all required projects with proper dependencies
- Configure project references and package management
- Establish folder structure for CQRS pattern

### Tasks

#### Day 1: Solution & Core Projects
- [ ] Create blank solution `dotnet-saas-multitenant-api`
- [ ] Create `Core/Domain` class library (.NET 10)
- [ ] Create `Core/Application` class library (.NET 10)
- [ ] Create `Infrastructure` class library (.NET 10)
- [ ] Create `Presentation/API` web API project (.NET 10)
- [ ] Create `Tests` folder structure:
  - [ ] `Domain.UnitTests`
  - [ ] `Application.UnitTests`
  - [ ] `Application.IntegrationTests`
  - [ ] `API.IntegrationTests`
- [ ] Configure project references:
  - Domain: No dependencies (pure)
  - Application: References Domain
  - Infrastructure: References Application
  - API: References Infrastructure
  - Tests: Reference respective layers

#### Day 2: Package Installation
- [ ] Install Domain packages:
  - None (keep pure)
- [ ] Install Application packages:
  - MediatR (v12.x)
  - FluentValidation (v11.x)
  - AutoMapper (v12.x)
- [ ] Install Infrastructure packages:
  - Microsoft.EntityFrameworkCore (v9.x)
  - Microsoft.EntityFrameworkCore.Design
  - Npgsql.EntityFrameworkCore.PostgreSQL (v9.x)
  - Microsoft.AspNetCore.Authentication.JwtBearer
  - BCrypt.Net-Next
  - StackExchange.Redis
- [ ] Install API packages:
  - Swashbuckle.AspNetCore (v6.x)
  - Serilog.AspNetCore
  - Serilog.Sinks.Console
- [ ] Install Test packages:
  - xUnit (v2.x)
  - Moq (v4.x)
  - FluentAssertions (v6.x)
  - Microsoft.EntityFrameworkCore.InMemory

#### Day 3: Folder Structure & Base Files
- [ ] Create Domain folder structure:
  ```
  Domain/
  ├── Entities/
  ├── ValueObjects/
  ├── Events/
  ├── Repositories/
  ├── Services/
  ├── Exceptions/
  └── Common/
  ```
- [ ] Create Application folder structure:
  ```
  Application/
  ├── Common/
  │   ├── Behaviors/
  │   ├── Interfaces/
  │   ├── Mappings/
  │   └── Models/
  └── Features/
      ├── Auth/
      │   ├── Commands/
      │   └── Queries/
      ├── Users/
      ├── Tenants/
      └── Roles/
  ```
- [ ] Create Infrastructure folder structure:
  ```
  Infrastructure/
  ├── Persistence/
  │   ├── Configurations/
  │   ├── Interceptors/
  │   ├── Repositories/
  │   └── Migrations/
  ├── Identity/
  ├── Multitenancy/
  ├── Caching/
  └── Services/
  ```
- [ ] Create `.gitignore` for .NET projects
- [ ] Create `README.md` in each project explaining purpose
- [ ] Initial commit to source control

### Success Criteria
- ✅ Solution builds successfully
- ✅ All projects reference correct dependencies
- ✅ No circular dependencies
- ✅ Folder structure matches Clean Architecture
- ✅ All packages installed and compatible

### Risks & Mitigation
- **Risk**: Package version conflicts
  - **Mitigation**: Use Central Package Management
- **Risk**: Wrong project dependencies
  - **Mitigation**: Review architecture diagram before linking

### Notes
- Keep Domain project completely pure (no external dependencies)
- Ensure proper naming conventions from the start
- Document any deviations from standard structure

---

## Phase 2: Domain Entities & Value Objects

**Duration**: 4 days  
**Status**: ⚪ Not Started  
**Priority**: 🔴 Critical  
**Dependencies**: Phase 1

### Objectives
- Create rich domain entities with business logic
- Implement value objects for type safety
- Define domain events for significant occurrences
- Establish repository interfaces
- Create domain exceptions

### Tasks

#### Day 1: Base Classes & Common Types
- [ ] Create `Domain/Common/BaseEntity.cs`:
  ```csharp
  public abstract class BaseEntity
  {
      public Guid Id { get; protected set; }
      public DateTime CreatedAt { get; protected set; }
      public DateTime? UpdatedAt { get; protected set; }
      public bool IsDeleted { get; protected set; }
  }
  ```
- [ ] Create `Domain/Common/AggregateRoot.cs`:
  ```csharp
  public abstract class AggregateRoot : BaseEntity
  {
      private readonly List<IDomainEvent> _domainEvents = new();
      public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
      protected void AddDomainEvent(IDomainEvent @event) { }
      public void ClearDomainEvents() { }
  }
  ```
- [ ] Create `Domain/Common/ValueObject.cs` (abstract base)
- [ ] Create `Domain/Common/Result.cs`:
  ```csharp
  public class Result<T>
  {
      public bool IsSuccess { get; }
      public T Value { get; }
      public string Error { get; }
  }
  ```
- [ ] Create `Domain/Common/Error.cs`

#### Day 2: Value Objects
- [ ] Create `Domain/ValueObjects/Email.cs`:
  - Validation logic
  - Factory method
  - Immutability
- [ ] Create `Domain/ValueObjects/TenantId.cs`:
  - Strong typing for tenant IDs
  - Validation
- [ ] Create `Domain/ValueObjects/Password.cs`:
  - Password strength validation
  - Security rules
- [ ] Create `Domain/ValueObjects/SubscriptionTier.cs`:
  - Enum-like value object
  - Business rules per tier

#### Day 3: Core Entities
- [ ] Create `Domain/Entities/User.cs`:
  - Properties (Id, TenantId, Email, PasswordHash, etc.)
  - Private setters for encapsulation
  - Factory method: `Create()`
  - Business methods: `AssignRole()`, `Deactivate()`, `UpdatePassword()`
  - Navigation properties with backing fields
  - Domain event raising
- [ ] Create `Domain/Entities/Tenant.cs`:
  - Properties (Id, Name, Subdomain, IsActive, etc.)
  - Factory method: `Create()`
  - Business methods: `Activate()`, `Deactivate()`, `UpdateSettings()`
- [ ] Create `Domain/Entities/Role.cs`:
  - Properties (Id, TenantId, Name, IsSystemRole)
  - Factory method: `Create()`
  - Business methods: `AssignPermission()`, `RevokePermission()`
- [ ] Create `Domain/Entities/Permission.cs`:
  - Properties (Id, Name, Resource, Action)
  - Static methods for common permissions

#### Day 4: Domain Events, Exceptions & Interfaces
- [ ] Create `Domain/Events/IDomainEvent.cs` interface
- [ ] Create domain events:
  - `UserCreatedEvent.cs`
  - `UserDeactivatedEvent.cs`
  - `RoleAssignedEvent.cs`
  - `TenantProvisionedEvent.cs`
  - `PasswordChangedEvent.cs`
- [ ] Create domain exceptions:
  - `Domain/Exceptions/DomainException.cs` (base)
  - `TenantNotFoundException.cs`
  - `UserNotFoundException.cs`
  - `UserAlreadyExistsException.cs`
  - `InvalidOperationException.cs`
- [ ] Create repository interfaces:
  - `Domain/Repositories/IUserRepository.cs`
  - `Domain/Repositories/ITenantRepository.cs`
  - `Domain/Repositories/IRoleRepository.cs`
  - `Domain/Repositories/IUnitOfWork.cs`
- [ ] Create domain service interfaces:
  - `Domain/Services/ITenantIsolationService.cs`
  - `Domain/Services/IPasswordHashingService.cs`

### Success Criteria
- ✅ All entities have proper encapsulation (private setters)
- ✅ Value objects are immutable
- ✅ Factory methods enforce business rules
- ✅ Domain events properly defined
- ✅ No infrastructure concerns in Domain layer
- ✅ Domain compiles without external dependencies

### Risks & Mitigation
- **Risk**: Anemic domain model (entities as data containers)
  - **Mitigation**: Review each entity for missing business logic
- **Risk**: Breaking encapsulation
  - **Mitigation**: Use private setters, expose methods not properties

### Notes
- Focus on business logic, not data access
- Entities should protect their invariants
- Value objects enforce type safety

---

## Phase 3: Application Layer Setup (MediatR)

**Duration**: 3 days  
**Status**: ⚪ Not Started  
**Priority**: 🔴 Critical  
**Dependencies**: Phase 2

### Objectives
- Configure MediatR for CQRS implementation
- Set up FluentValidation infrastructure
- Configure AutoMapper profiles
- Create common interfaces and models
- Establish marker interfaces for Commands/Queries

### Tasks

#### Day 1: MediatR & Common Interfaces
- [ ] Create `Application/DependencyInjection.cs`:
  - Register MediatR from assemblies
  - Configure FluentValidation
  - Register AutoMapper
- [ ] Create marker interfaces:
  - `Application/Common/Interfaces/ICommand.cs`
  - `Application/Common/Interfaces/IQuery.cs`
- [ ] Create common interfaces:
  - `IApplicationDbContext.cs`:
    ```csharp
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Tenant> Tenants { get; }
        DbSet<Role> Roles { get; }
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
    ```
  - `ICurrentUserService.cs`:
    ```csharp
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string Email { get; }
        Guid? TenantId { get; }
        bool IsAuthenticated { get; }
    }
    ```
  - `ITenantContext.cs`:
    ```csharp
    public interface ITenantContext
    {
        Guid TenantId { get; }
        string TenantName { get; }
        bool IsResolved { get; }
    }
    ```
  - `IDateTime.cs` (for testability)

#### Day 2: Common Models & DTOs
- [ ] Create `Application/Common/Models/PaginatedList.cs`:
  ```csharp
  public class PaginatedList<T>
  {
      public List<T> Items { get; }
      public int PageNumber { get; }
      public int TotalPages { get; }
      public int TotalCount { get; }
      public bool HasPreviousPage { get; }
      public bool HasNextPage { get; }
  }
  ```
- [ ] Create `Application/Common/Models/ApiResponse.cs`:
  ```csharp
  public class ApiResponse<T>
  {
      public bool Success { get; set; }
      public T Data { get; set; }
      public string Message { get; set; }
      public List<string> Errors { get; set; }
  }
  ```
- [ ] Create common DTOs:
  - `ValidationError.cs`
  - `ErrorDetails.cs`
- [ ] Create `Application/Common/Exceptions/ValidationException.cs`:
  ```csharp
  public class ValidationException : Exception
  {
      public List<ValidationFailure> Errors { get; }
  }
  ```
- [ ] Create `Application/Common/Exceptions/NotFoundException.cs`
- [ ] Create `Application/Common/Exceptions/ForbiddenAccessException.cs`

#### Day 3: AutoMapper & Feature Folders
- [ ] Create `Application/Common/Mappings/MappingProfile.cs`:
  - Configure entity to DTO mappings
  - Set up reverse mappings where needed
- [ ] Create `Application/Common/Mappings/IMapFrom.cs` interface:
  ```csharp
  public interface IMapFrom<T>
  {
      void Mapping(Profile profile) => 
          profile.CreateMap(typeof(T), GetType());
  }
  ```
- [ ] Set up feature folder structure:
  ```
  Features/
  ├── Auth/
  │   ├── Commands/
  │   │   ├── Login/
  │   │   ├── Register/
  │   │   └── RefreshToken/
  │   └── Queries/
  ├── Users/
  │   ├── Commands/
  │   │   ├── CreateUser/
  │   │   ├── UpdateUser/
  │   │   └── DeleteUser/
  │   └── Queries/
  │       ├── GetUsers/
  │       └── GetUserById/
  ├── Tenants/
  │   ├── Commands/
  │   └── Queries/
  └── Roles/
      ├── Commands/
      └── Queries/
  ```
- [ ] Document CQRS folder conventions in README

### Success Criteria
- ✅ MediatR properly registered and configured
- ✅ Common interfaces defined and documented
- ✅ AutoMapper configured with profiles
- ✅ Feature folder structure established
- ✅ Application layer compiles successfully

### Risks & Mitigation
- **Risk**: Over-complicated abstractions
  - **Mitigation**: Keep interfaces simple and focused
- **Risk**: Inconsistent folder structure
  - **Mitigation**: Document and enforce conventions

### Notes
- Keep Application layer focused on orchestration
- No infrastructure concerns (database, HTTP, etc.)
- All external dependencies as interfaces

---

## Phase 4: Infrastructure - EF Core Setup

**Duration**: 4 days  
**Status**: ⚪ Not Started  
**Priority**: 🔴 Critical  
**Dependencies**: Phase 3

### Objectives
- Configure Entity Framework Core with PostgreSQL
- Create DbContext implementations
- Configure entity mappings
- Implement repository pattern
- Set up EF Core interceptors

### Tasks

#### Day 1: DbContext Setup
- [ ] Create `Infrastructure/Persistence/ApplicationDbContext.cs`:
  ```csharp
  public class ApplicationDbContext : DbContext, IApplicationDbContext
  {
      private readonly ITenantContext _tenantContext;
      
      public DbSet<User> Users { get; set; }
      public DbSet<Tenant> Tenants { get; set; }
      public DbSet<Role> Roles { get; set; }
      public DbSet<Permission> Permissions { get; set; }
      
      protected override void OnModelCreating(ModelBuilder builder)
      {
          // Apply configurations
          // Set up global query filters
      }
  }
  ```
- [ ] Create connection string configuration in appsettings
- [ ] Configure DbContext service registration
- [ ] Test database connection

#### Day 2: Entity Configurations
- [ ] Create `Infrastructure/Persistence/Configurations/UserConfiguration.cs`:
  ```csharp
  public class UserConfiguration : IEntityTypeConfiguration<User>
  {
      public void Configure(EntityTypeBuilder<User> builder)
      {
          builder.ToTable("Users");
          builder.HasKey(u => u.Id);
          builder.Property(u => u.Email)
              .HasConversion(e => e.Value, v => Email.Create(v).Value);
          // Configure relationships, indexes, etc.
      }
  }
  ```
- [ ] Create configurations:
  - `TenantConfiguration.cs`
  - `RoleConfiguration.cs`
  - `PermissionConfiguration.cs`
  - `UserRoleConfiguration.cs` (join table)
  - `RolePermissionConfiguration.cs` (join table)
- [ ] Configure value object conversions
- [ ] Set up indexes and constraints
- [ ] Configure cascade delete behavior

#### Day 3: Interceptors & Repository Implementation
- [ ] Create `Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs`:
  ```csharp
  public class AuditableEntityInterceptor : SaveChangesInterceptor
  {
      public override InterceptionResult<int> SavingChanges(...)
      {
          // Auto-set CreatedAt, UpdatedAt, CreatedBy
      }
  }
  ```
- [ ] Create `Infrastructure/Persistence/Interceptors/SoftDeleteInterceptor.cs`:
  ```csharp
  public class SoftDeleteInterceptor : SaveChangesInterceptor
  {
      public override InterceptionResult<int> SavingChanges(...)
      {
          // Convert delete to soft delete (IsDeleted = true)
      }
  }
  ```
- [ ] Create `Infrastructure/Persistence/Repositories/UserRepository.cs`:
  ```csharp
  public class UserRepository : IUserRepository
  {
      private readonly ApplicationDbContext _context;
      
      public async Task<User> GetByIdAsync(Guid id, CancellationToken ct)
      {
          return await _context.Users
              .Include(u => u.Roles)
              .FirstOrDefaultAsync(u => u.Id == id, ct);
      }
      // Other methods...
  }
  ```
- [ ] Create repository implementations:
  - `TenantRepository.cs`
  - `RoleRepository.cs`

#### Day 4: Unit of Work & Initial Migration
- [ ] Create `Infrastructure/Persistence/UnitOfWork.cs`:
  ```csharp
  public class UnitOfWork : IUnitOfWork
  {
      private readonly ApplicationDbContext _context;
      
      public IUserRepository Users { get; }
      public ITenantRepository Tenants { get; }
      public IRoleRepository Roles { get; }
      
      public async Task<int> SaveChangesAsync(CancellationToken ct)
      {
          return await _context.SaveChangesAsync(ct);
      }
      
      public async Task BeginTransactionAsync(CancellationToken ct)
      {
          await _context.Database.BeginTransactionAsync(ct);
      }
  }
  ```
- [ ] Create `Infrastructure/DependencyInjection.cs`:
  - Register DbContext
  - Register repositories
  - Register Unit of Work
  - Register interceptors
- [ ] Create initial EF Core migration:
  ```bash
  dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
  ```
- [ ] Review generated migration
- [ ] Test migration on local PostgreSQL

### Success Criteria
- ✅ DbContext properly configured with all entities
- ✅ Entity configurations use Fluent API
- ✅ Repositories implement interfaces from Domain
- ✅ Interceptors work for audit and soft delete
- ✅ Initial migration creates correct schema
- ✅ Can connect to PostgreSQL database

### Risks & Mitigation
- **Risk**: Migration conflicts
  - **Mitigation**: Review migrations before applying
- **Risk**: Incorrect entity relationships
  - **Mitigation**: Test with sample data
- **Risk**: Performance issues with query filters
  - **Mitigation**: Profile queries, use indexes

### Notes
- Use explicit configuration over conventions
- Always include navigation properties in queries when needed
- Test soft delete and audit interceptors thoroughly

---

## Phase 5: Multi-Tenancy Infrastructure

**Duration**: 5 days  
**Status**: ⚪ Not Started  
**Priority**: 🔴 Critical  
**Dependencies**: Phase 4

### Objectives
- Implement tenant resolution strategy
- Create tenant context service
- Set up global query filters for tenant isolation
- Build tenant middleware
- Ensure complete data isolation

### Tasks

#### Day 1: Tenant Context & Resolution Strategy
- [ ] Create `Infrastructure/Multitenancy/TenantContext.cs`:
  ```csharp
  public class TenantContext : ITenantContext
  {
      public Guid TenantId { get; private set; }
      public string TenantName { get; private set; }
      public bool IsResolved { get; private set; }
      
      public void SetTenant(Guid tenantId, string tenantName)
      {
          TenantId = tenantId;
          TenantName = tenantName;
          IsResolved = true;
      }
  }
  ```
- [ ] Register TenantContext as scoped service
- [ ] Create `Infrastructure/Multitenancy/TenantResolver.cs`:
  ```csharp
  public interface ITenantResolver
  {
      Task<Tenant> ResolveTenantAsync(HttpContext context);
  }
  
  public class TenantResolver : ITenantResolver
  {
      // Resolve from header: X-Tenant-Id
      // Resolve from subdomain: {tenant}.domain.com
      // Resolve from route: /tenants/{tenantId}/...
  }
  ```
- [ ] Implement multiple resolution strategies:
  - Header-based resolution
  - Subdomain-based resolution
  - Route parameter resolution
- [ ] Add configuration for resolution strategy

#### Day 2: Tenant Middleware
- [ ] Create `Infrastructure/Multitenancy/TenantMiddleware.cs`:
  ```csharp
  public class TenantMiddleware
  {
      private readonly RequestDelegate _next;
      private readonly ITenantResolver _resolver;
      private readonly ITenantContext _tenantContext;
      
      public async Task InvokeAsync(HttpContext context)
      {
          var tenant = await _resolver.ResolveTenantAsync(context);
          
          if (tenant == null)
          {
              context.Response.StatusCode = 400;
              await context.Response.WriteAsync("Tenant not specified");
              return;
          }
          
          _tenantContext.SetTenant(tenant.Id, tenant.Name);
          await _next(context);
      }
  }
  ```
- [ ] Add middleware extension method
- [ ] Configure middleware in API pipeline
- [ ] Add bypass logic for:
  - Health check endpoints
  - Authentication endpoints
  - Swagger endpoints
  - Super admin endpoints

#### Day 3: Global Query Filters
- [ ] Update `ApplicationDbContext` with query filters:
  ```csharp
  protected override void OnModelCreating(ModelBuilder builder)
  {
      base.OnModelCreating(builder);
      
      // Tenant isolation filter
      builder.Entity<User>().HasQueryFilter(u => 
          u.TenantId == _tenantContext.TenantId);
      
      builder.Entity<Role>().HasQueryFilter(r => 
          r.TenantId == _tenantContext.TenantId || r.IsSystemRole);
      
      // Soft delete filter
      builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
      
      // Apply configurations
      builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
  ```
- [ ] Add query filter bypass method:
  ```csharp
  public IQueryable<T> GetAllIncludingDeleted<T>() where T : BaseEntity
  {
      return _context.Set<T>().IgnoreQueryFilters();
  }
  ```
- [ ] Test query filters with multiple tenants
- [ ] Verify tenant isolation in queries

#### Day 4: Tenant Service & Provisioning
- [ ] Create `Infrastructure/Multitenancy/TenantService.cs`:
  ```csharp
  public class TenantService : ITenantService
  {
      public async Task<Tenant> CreateTenantAsync(string name, string subdomain)
      {
          // Create tenant record
          // Optionally create separate database/schema
          // Seed default data (roles, permissions)
      }
      
      public async Task ProvisionTenantAsync(Guid tenantId)
      {
          // Run migrations for tenant
          // Seed initial data
          // Create default admin user
      }
      
      public async Task<bool> ValidateTenantAsync(Guid tenantId)
      {
          // Check if tenant exists and is active
      }
  }
  ```
- [ ] Create default data seeder per tenant:
  - System roles (Admin, User)
  - Default permissions
  - Sample configuration
- [ ] Implement tenant database strategy:
  - Shared database with discriminator (initial approach)
  - Document how to switch to database-per-tenant
- [ ] Test tenant provisioning flow

#### Day 5: Testing & Documentation
- [ ] Write unit tests for tenant resolution:
  - Test header-based resolution
  - Test subdomain resolution
  - Test missing tenant handling
- [ ] Write integration tests for tenant isolation:
  - Create data for Tenant A
  - Query as Tenant B
  - Verify no data returned
- [ ] Test cross-tenant data access prevention
- [ ] Document multi-tenancy architecture in `docs/MULTITENANCY.md`:
  - Resolution strategies
  - Query filters explanation
  - How to bypass filters (for super admin)
  - Tenant provisioning process
  - Migration to database-per-tenant
- [ ] Create troubleshooting guide for tenant issues

### Success Criteria
- ✅ Tenant resolution works for all strategies
- ✅ Middleware correctly resolves tenant
- ✅ Query filters prevent cross-tenant access
- ✅ Can create and provision new tenants
- ✅ Integration tests prove isolation
- ✅ Documentation complete

### Risks & Mitigation
- **Risk**: Accidental cross-tenant data access
  - **Mitigation**: Comprehensive integration tests
- **Risk**: Performance impact of query filters
  - **Mitigation**: Index tenant columns, monitor queries
- **Risk**: Complex tenant resolution logic
  - **Mitigation**: Start simple (header-based), add complexity later

### Notes
- Always test tenant isolation thoroughly
- Consider performance implications of query filters
- Document how to bypass filters for admin operations

---

## Phase 6: Authentication & JWT

**Duration**: 5 days  
**Status**: ⚪ Not Started  
**Priority**: 🔴 Critical  
**Dependencies**: Phase 5

### Objectives
- Implement JWT token generation and validation
- Create password hashing service
- Build refresh token mechanism
- Configure ASP.NET Core authentication
- Implement current user service

### Tasks

#### Day 1: JWT Configuration & Token Generator
- [ ] Add JWT settings to `appsettings.json`:
  ```json
  {
    "Jwt": {
      "SecretKey": "your-secret-key-min-32-characters-long",
      "Issuer": "dotnet-saas-api",
      "Audience": "dotnet-saas-client",
      "AccessTokenExpirationMinutes": 30,
      "RefreshTokenExpirationDays": 7
    }
  }
  ```
- [ ] Create `Infrastructure/Identity/JwtSettings.cs`:
  ```csharp
  public class JwtSettings
  {
      public string SecretKey { get; set; }
      public string Issuer { get; set; }
      public string Audience { get; set; }
      public int AccessTokenExpirationMinutes { get; set; }
      public int RefreshTokenExpirationDays { get; set; }
  }
  ```
- [ ] Create `Infrastructure/Identity/JwtTokenGenerator.cs`:
  ```csharp
  public class JwtTokenGenerator : IJwtTokenGenerator
  {
      public string GenerateAccessToken(User user, List<string> roles)
      {
          var claims = new List<Claim>
          {
              new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
              new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
              new Claim("tenantId", user.TenantId.Value.ToString()),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
          };
          
          foreach (var role in roles)
          {
              claims.Add(new Claim(ClaimTypes.Role, role));
          }
          
          var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
          var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
          
          var token = new JwtSecurityToken(
              issuer: _settings.Issuer,
              audience: _settings.Audience,
              claims: claims,
              expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
              signingCredentials: credentials
          );
          
          return new JwtSecurityTokenHandler().WriteToken(token);
      }
      
      public string GenerateRefreshToken()
      {
          return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
      }
  }
  ```
- [ ] Test token generation with sample user

#### Day 2: Password Hashing & Validation
- [ ] Create `Infrastructure/Identity/PasswordHasher.cs`:
  ```csharp
  public class PasswordHasher : IPasswordHasher
  {
      public string HashPassword(string password)
      {
          return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
      }
      
      public bool VerifyPassword(string password, string passwordHash)
      {
          return BCrypt.Net.BCrypt.Verify(password, passwordHash);
      }
  }
  ```
- [ ] Create password validation rules:
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 number
  - At least 1 special character
- [ ] Add password strength validator
- [ ] Test password hashing and verification

#### Day 3: Refresh Token Entity & Repository
- [ ] Create `Domain/Entities/RefreshToken.cs`:
  ```csharp
  public class RefreshToken : BaseEntity
  {
      public Guid UserId { get; private set; }
      public string Token { get; private set; }
      public DateTime ExpiresAt { get; private set; }
      public DateTime? RevokedAt { get; private set; }
      public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
      public bool IsRevoked => RevokedAt.HasValue;
      public bool IsActive => !IsExpired && !IsRevoked;
      
      public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
      {
          return new RefreshToken
          {
              Id = Guid.NewGuid(),
              UserId = userId,
              Token = token,
              ExpiresAt = expiresAt,
              CreatedAt = DateTime.UtcNow
          };
      }
      
      public void Revoke()
      {
          RevokedAt = DateTime.UtcNow;
      }
  }
  ```
- [ ] Add RefreshToken DbSet to ApplicationDbContext
- [ ] Create EF Core configuration for RefreshToken
- [ ] Create migration for RefreshToken table
- [ ] Create `IRefreshTokenRepository` interface
- [ ] Implement RefreshTokenRepository

#### Day 4: ASP.NET Core Authentication Configuration
- [ ] Configure authentication in API `Program.cs`:
  ```csharp
  builder.Services.AddAuthentication(options =>
  {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  })
  .AddJwtBearer(options =>
  {
      var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
      
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtSettings.Issuer,
          ValidAudience = jwtSettings.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
          ClockSkew = TimeSpan.Zero
      };
      
      options.Events = new JwtBearerEvents
      {
          OnAuthenticationFailed = context =>
          {
              // Log authentication failures
              return Task.CompletedTask;
          },
          OnTokenValidated = context =>
          {
              // Additional validation logic
              return Task.CompletedTask;
          }
      };
  });
  
  builder.Services.AddAuthorization();
  ```
- [ ] Add authentication middleware to pipeline
- [ ] Test JWT validation with valid and invalid tokens

#### Day 5: Current User Service & Testing
- [ ] Create `Infrastructure/Identity/CurrentUserService.cs`:
  ```csharp
  public class CurrentUserService : ICurrentUserService
  {
      private readonly IHttpContextAccessor _httpContextAccessor;
      
      public Guid? UserId
      {
          get
          {
              var userIdClaim = _httpContextAccessor.HttpContext?.User?
                  .FindFirst(ClaimTypes.NameIdentifier)?.Value;
              
              return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
          }
      }
      
      public string Email => 
          _httpContextAccessor.HttpContext?.User?
              .FindFirst(ClaimTypes.Email)?.Value;
      
      public Guid? TenantId
      {
          get
          {
              var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
                  .FindFirst("tenantId")?.Value;
              
              return tenantIdClaim != null ? Guid.Parse(tenantIdClaim) : null;
          }
      }
      
      public bool IsAuthenticated => 
          _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
  }
  ```
- [ ] Register CurrentUserService as scoped
- [ ] Register HttpContextAccessor
- [ ] Update Infrastructure DependencyInjection with all auth services
- [ ] Write unit tests for JwtTokenGenerator
- [ ] Write unit tests for PasswordHasher
- [ ] Document authentication flow in `docs/AUTHENTICATION.md`:
  - JWT structure and claims
  - Token generation process
  - Token validation process
  - Refresh token flow
  - Password hashing details

### Success Criteria
- ✅ JWT tokens generated with correct claims
- ✅ Token validation works correctly
- ✅ Password hashing is secure (BCrypt with work factor 12)
- ✅ Refresh token mechanism implemented
- ✅ Current user service retrieves claims from token
- ✅ Authentication middleware configured
- ✅ Unit tests pass

### Risks & Mitigation
- **Risk**: Weak JWT secret key
  - **Mitigation**: Validate key length, use environment variables
- **Risk**: Token expiration issues
  - **Mitigation**: Handle clock skew, test timezone scenarios
- **Risk**: Password hash vulnerabilities
  - **Mitigation**: Use BCrypt with high work factor

### Notes
- Never store JWT secret in source control
- Use asymmetric encryption (RS256) for production
- Implement token blacklist for revocation
- Consider token versioning for security updates

---

## Phase 7: Auth Feature - CQRS Implementation

**Duration**: 4 days  
**Status**: ⚪ Not Started  
**Priority**: 🟠 High  
**Dependencies**: Phase 6

### Objectives
- Implement Login command and handler
- Implement Register command and handler
- Implement RefreshToken command and handler
- Create validators for all commands
- Build DTOs and responses

### Tasks

#### Day 1: Login Command
- [ ] Create `Application/Features/Auth/Commands/Login/LoginCommand.cs`:
  ```csharp
  public record LoginCommand : ICommand<Result<LoginResponse>>
  {
      public string Email { get; init; }
      public string Password { get; init; }
  }
  ```
- [ ] Create `LoginResponse.cs`:
  ```csharp
  public record LoginResponse
  {
      public string AccessToken { get; init; }
      public string RefreshToken { get; init; }
      public DateTime ExpiresAt { get; init; }
      public UserDto User { get; init; }
  }
  ```
- [ ] Create `LoginCommandValidator.cs`:
  ```csharp
  public class LoginCommandValidator : AbstractValidator<LoginCommand>
  {
      public LoginCommandValidator()
      {
          RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required")
              .EmailAddress().WithMessage("Invalid email format");
          
          RuleFor(x => x.Password)
              .NotEmpty().WithMessage("Password is required");
      }
  }
  ```
- [ ] Create `LoginCommandHandler.cs`:
  ```csharp
  public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
  {
      private readonly IUserRepository _userRepository;
      private readonly IPasswordHasher _passwordHasher;
      private readonly IJwtTokenGenerator _tokenGenerator;
      private readonly IRefreshTokenRepository _refreshTokenRepository;
      private readonly IUnitOfWork _unitOfWork;
      
      public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
      {
          // 1. Get user by email
          var emailResult = Email.Create(request.Email);
          if (emailResult.IsFailure)
              return Result<LoginResponse>.Failure(emailResult.Error);
          
          var user = await _userRepository.GetByEmailAsync(emailResult.Value, ct);
          if (user == null)
              return Result<LoginResponse>.Failure("Invalid credentials");
          
          // 2. Verify password
          if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
              return Result<LoginResponse>.Failure("Invalid credentials");
          
          // 3. Check if user is active
          if (!user.IsActive)
              return Result<LoginResponse>.Failure("User account is deactivated");
          
          // 4. Generate tokens
          var roles = user.Roles.Select(r => r.Name).ToList();
          var accessToken = _tokenGenerator.GenerateAccessToken(user, roles);
          var refreshToken = _tokenGenerator.GenerateRefreshToken();
          
          // 5. Store refresh token
          var refreshTokenEntity = RefreshToken.Create(
              user.Id, 
              refreshToken, 
              DateTime.UtcNow.AddDays(7));
          
          await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);
          await _unitOfWork.SaveChangesAsync(ct);
          
          // 6. Update last login
          user.UpdateLastLogin();
          await _unitOfWork.SaveChangesAsync(ct);
          
          // 7. Return response
          return Result<LoginResponse>.Success(new LoginResponse
          {
              AccessToken = accessToken,
              RefreshToken = refreshToken,
              ExpiresAt = DateTime.UtcNow.AddMinutes(30),
              User = _mapper.Map<UserDto>(user)
          });
      }
  }
  ```
- [ ] Test login with valid and invalid credentials

#### Day 2: Register Command
- [ ] Create `Application/Features/Auth/Commands/Register/RegisterCommand.cs`:
  ```csharp
  public record RegisterCommand : ICommand<Result<RegisterResponse>>
  {
      public Guid TenantId { get; init; }
      public string Email { get; init; }
      public string Password { get; init; }
      public string ConfirmPassword { get; init; }
      public string FirstName { get; init; }
      public string LastName { get; init; }
  }
  ```
- [ ] Create `RegisterResponse.cs`:
  ```csharp
  public record RegisterResponse
  {
      public Guid UserId { get; init; }
      public string Email { get; init; }
      public string Message { get; init; }
  }
  ```
- [ ] Create `RegisterCommandValidator.cs`:
  ```csharp
  public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
  {
      public RegisterCommandValidator()
      {
          RuleFor(x => x.TenantId)
              .NotEmpty().WithMessage("TenantId is required");
          
          RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required")
              .EmailAddress().WithMessage("Invalid email format");
          
          RuleFor(x => x.Password)
              .NotEmpty().WithMessage("Password is required")
              .MinimumLength(8).WithMessage("Password must be at least 8 characters")
              .Matches(@"[A-Z]").WithMessage("Password must contain uppercase")
              .Matches(@"[a-z]").WithMessage("Password must contain lowercase")
              .Matches(@"[0-9]").WithMessage("Password must contain number")
              .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain special character");
          
          RuleFor(x => x.ConfirmPassword)
              .Equal(x => x.Password).WithMessage("Passwords do not match");
          
          RuleFor(x => x.FirstName)
              .NotEmpty().WithMessage("First name is required")
              .MaximumLength(50);
          
          RuleFor(x => x.LastName)
              .NotEmpty().WithMessage("Last name is required")
              .MaximumLength(50);
      }
  }
  ```
- [ ] Create `RegisterCommandHandler.cs`:
  ```csharp
  public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
  {
      public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken ct)
      {
          // 1. Validate tenant exists
          var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
          if (tenant == null)
              return Result<RegisterResponse>.Failure("Tenant not found");
          
          // 2. Check if user already exists
          var emailResult = Email.Create(request.Email);
          var existingUser = await _userRepository.GetByEmailAsync(emailResult.Value, ct);
          if (existingUser != null)
              return Result<RegisterResponse>.Failure("User with this email already exists");
          
          // 3. Hash password
          var passwordHash = _passwordHasher.HashPassword(request.Password);
          
          // 4. Create user
          var tenantId = TenantId.Create(request.TenantId);
          var user = User.Create(tenantId, emailResult.Value, passwordHash);
          user.SetName(request.FirstName, request.LastName);
          
          // 5. Assign default role
          var defaultRole = await _roleRepository.GetByNameAsync("User", ct);
          if (defaultRole != null)
          {
              user.AssignRole(defaultRole);
          }
          
          // 6. Save user
          await _userRepository.AddAsync(user, ct);
          await _unitOfWork.SaveChangesAsync(ct);
          
          return Result<RegisterResponse>.Success(new RegisterResponse
          {
              UserId = user.Id,
              Email = user.Email.Value,
              Message = "User registered successfully"
          });
      }
  }
  ```
- [ ] Test registration flow

#### Day 3: RefreshToken Command
- [ ] Create `Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommand.cs`:
  ```csharp
  public record RefreshTokenCommand : ICommand<Result<RefreshTokenResponse>>
  {
      public string RefreshToken { get; init; }
  }
  ```
- [ ] Create `RefreshTokenResponse.cs`:
  ```csharp
  public record RefreshTokenResponse
  {
      public string AccessToken { get; init; }
      public string RefreshToken { get; init; }
      public DateTime ExpiresAt { get; init; }
  }
  ```
- [ ] Create `RefreshTokenCommandValidator.cs`
- [ ] Create `RefreshTokenCommandHandler.cs`:
  ```csharp
  public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
  {
      public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
      {
          // 1. Find refresh token
          var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);
          if (storedToken == null)
              return Result<RefreshTokenResponse>.Failure("Invalid refresh token");
          
          // 2. Validate token
          if (!storedToken.IsActive)
              return Result<RefreshTokenResponse>.Failure("Refresh token expired or revoked");
          
          // 3. Get user
          var user = await _userRepository.GetByIdAsync(storedToken.UserId, ct);
          if (user == null || !user.IsActive)
              return Result<RefreshTokenResponse>.Failure("User not found or inactive");
          
          // 4. Revoke old token
          storedToken.Revoke();
          
          // 5. Generate new tokens
          var roles = user.Roles.Select(r => r.Name).ToList();
          var newAccessToken = _tokenGenerator.GenerateAccessToken(user, roles);
          var newRefreshToken = _tokenGenerator.GenerateRefreshToken();
          
          // 6. Store new refresh token
          var newRefreshTokenEntity = RefreshToken.Create(
              user.Id,
              newRefreshToken,
              DateTime.UtcNow.AddDays(7));
          
          await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, ct);
          await _unitOfWork.SaveChangesAsync(ct);
          
          return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse
          {
              AccessToken = newAccessToken,
              RefreshToken = newRefreshToken,
              ExpiresAt = DateTime.UtcNow.AddMinutes(30)
          });
      }
  }
  ```
- [ ] Test refresh token flow

#### Day 4: Additional Auth Commands & Testing
- [ ] Create `ForgotPassword` command structure (placeholder)
- [ ] Create `ResetPassword` command structure (placeholder)
- [ ] Create `ChangePassword` command:
  - Command, Validator, Handler
- [ ] Create `Logout` command:
  - Revoke refresh token
- [ ] Write unit tests:
  - Test successful login
  - Test failed login with wrong password
  - Test login with inactive user
  - Test successful registration
  - Test registration with existing email
  - Test refresh token flow
  - Test expired refresh token
- [ ] Document auth commands in code comments

### Success Criteria
- ✅ Login command works with valid credentials
- ✅ Login fails gracefully with invalid credentials
- ✅ Register command creates new users
- ✅ Refresh token command generates new tokens
- ✅ All validators enforce business rules
- ✅ Unit tests pass
- ✅ Tokens contain correct claims

### Risks & Mitigation
- **Risk**: Insecure password storage
  - **Mitigation**: Use BCrypt with high work factor
- **Risk**: Token replay attacks
  - **Mitigation**: Implement token revocation, short expiration
- **Risk**: Brute force attacks
  - **Mitigation**: Implement rate limiting (later phase)

### Notes
- Never log passwords or tokens
- Always validate user status before issuing tokens
- Consider implementing account lockout after failed attempts

---

## Phase 8: Users Feature - CQRS Implementation

**Duration**: 5 days  
**Status**: ⚪ Not Started  
**Priority**: 🟠 High  
**Dependencies**: Phase 7

### Objectives
- Implement user CRUD commands
- Implement user queries with pagination
- Create comprehensive validators
- Build user DTOs for different scenarios
- Implement role assignment commands

### Tasks

#### Day 1: Create & Update User Commands
- [ ] Create `Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs`:
  ```csharp
  public record CreateUserCommand : ICommand<Result<Guid>>
  {
      public string Email { get; init; }
      public string Password { get; init; }
      public string FirstName { get; init; }
      public string LastName { get; init; }
      public List<Guid> RoleIds { get; init; } = new();
  }
  ```
- [ ] Create `CreateUserCommandValidator.cs`
- [ ] Create `CreateUserCommandHandler.cs`
- [ ] Create `Application/Features/Users/Commands/UpdateUser/UpdateUserCommand.cs`:
  ```csharp
  public record UpdateUserCommand : ICommand<Result<Unit>>
  {
      public Guid Id { get; init; }
      public string FirstName { get; init; }
      public string LastName { get; init; }
      public bool? IsActive { get; init; }
  }
  ```
- [ ] Create `UpdateUserCommandValidator.cs`
- [ ] Create `UpdateUserCommandHandler.cs`:
  ```csharp
  public async Task<Result<Unit>> Handle(UpdateUserCommand request, CancellationToken ct)
  {
      var user = await _userRepository.GetByIdAsync(request.Id, ct);
      if (user == null)
          return Result<Unit>.Failure("User not found");
      
      // Update fields
      if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
      {
          user.SetName(request.FirstName ?? user.FirstName, request.LastName ?? user.LastName);
      }
      
      if (request.IsActive.HasValue)
      {
          if (request.IsActive.Value)
              user.Activate();
          else
              user.Deactivate();
      }
      
      await _unitOfWork.SaveChangesAsync(ct);
      return Result<Unit>.Success(Unit.Value);
  }
  ```
- [ ] Test create and update commands

#### Day 2: Delete User & Role Assignment Commands
- [ ] Create `Application/Features/Users/Commands/DeleteUser/DeleteUserCommand.cs`:
  ```csharp
  public record DeleteUserCommand : ICommand<Result<Unit>>
  {
      public Guid Id { get; init; }
  }
  ```
- [ ] Create `DeleteUserCommandHandler.cs` (soft delete)
- [ ] Create `Application/Features/Users/Commands/AssignRole/AssignRoleCommand.cs`:
  ```csharp
  public record AssignRoleCommand : ICommand<Result<Unit>>
  {
      public Guid UserId { get; init; }
      public Guid RoleId { get; init; }
  }
  ```
- [ ] Create `AssignRoleCommandValidator.cs`
- [ ] Create `AssignRoleCommandHandler.cs`:
  ```csharp
  public async Task<Result<Unit>> Handle(AssignRoleCommand request, CancellationToken ct)
  {
      var user = await _userRepository.GetByIdAsync(request.UserId, ct);
      if (user == null)
          return Result<Unit>.Failure("User not found");
      
      var role = await _roleRepository.GetByIdAsync(request.RoleId, ct);
      if (role == null)
          return Result<Unit>.Failure("Role not found");
      
      user.AssignRole(role);
      await _unitOfWork.SaveChangesAsync(ct);
      
      return Result<Unit>.Success(Unit.Value);
  }
  ```
- [ ] Create `RemoveRoleCommand` and handler
- [ ] Test role assignment

#### Day 3: User Query DTOs & Mappings
- [ ] Create `Application/Features/Users/Queries/GetUsers/UserDto.cs`:
  ```csharp
  public record UserDto : IMapFrom<User>
  {
      public Guid Id { get; init; }
      public string Email { get; init; }
      public string FirstName { get; init; }
      public string LastName { get; init; }
      public string FullName { get; init; }
      public bool IsActive { get; init; }
      public DateTime CreatedAt { get; init; }
      public List<string> Roles { get; init; } = new();
      
      public void Mapping(Profile profile)
      {
          profile.CreateMap<User, UserDto>()
              .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value))
              .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
              .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.Roles.Select(r => r.Name).ToList()));
      }
  }
  ```
- [ ] Create `UserDetailDto.cs`:
  ```csharp
  public record UserDetailDto : IMapFrom<User>
  {
      public Guid Id { get; init; }
      public Guid TenantId { get; init; }
      public string Email { get; init; }
      public string FirstName { get; init; }
      public string LastName { get; init; }
      public bool IsActive { get; init; }
      public bool EmailConfirmed { get; init; }
      public DateTime CreatedAt { get; init; }
      public DateTime? UpdatedAt { get; init; }
      public DateTime? LastLoginAt { get; init; }
      public List<RoleDto> Roles { get; init; } = new();
  }
  ```
- [ ] Configure AutoMapper mappings
- [ ] Test mapping from entities to DTOs

#### Day 4: User Queries
- [ ] Create `Application/Features/Users/Queries/GetUsers/GetUsersQuery.cs`:
  ```csharp
  public record GetUsersQuery : IQuery<Result<PaginatedList<UserDto>>>
  {
      public int PageNumber { get; init; } = 1;
      public int PageSize { get; init; } = 10;
      public string SearchTerm { get; init; }
      public bool? IsActive { get; init; }
      public string SortBy { get; init; } = "CreatedAt";
      public string SortOrder { get; init; } = "desc";
  }
  ```
- [ ] Create `GetUsersQueryValidator.cs`:
  ```csharp
  public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
  {
      public GetUsersQueryValidator()
      {
          RuleFor(x => x.PageNumber)
              .GreaterThan(0).WithMessage("Page number must be greater than 0");
          
          RuleFor(x => x.PageSize)
              .GreaterThan(0).WithMessage("Page size must be greater than 0")
              .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
          
          RuleFor(x => x.SortBy)
              .Must(x => new[] { "CreatedAt", "Email", "FirstName", "LastName" }.Contains(x))
              .When(x => !string.IsNullOrEmpty(x.SortBy))
              .WithMessage("Invalid sort field");
          
          RuleFor(x => x.SortOrder)
              .Must(x => new[] { "asc", "desc" }.Contains(x.ToLower()))
              .When(x => !string.IsNullOrEmpty(x.SortOrder))
              .WithMessage("Sort order must be 'asc' or 'desc'");
      }
  }
  ```
- [ ] Create `GetUsersQueryHandler.cs`:
  ```csharp
  public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PaginatedList<UserDto>>>
  {
      public async Task<Result<PaginatedList<UserDto>>> Handle(GetUsersQuery request, CancellationToken ct)
      {
          var query = _context.Users
              .Include(u => u.Roles)
              .AsQueryable();
          
          // Apply search filter
          if (!string.IsNullOrWhiteSpace(request.SearchTerm))
          {
              query = query.Where(u =>
                  u.Email.Value.Contains(request.SearchTerm) ||
                  u.FirstName.Contains(request.SearchTerm) ||
                  u.LastName.Contains(request.SearchTerm));
          }
          
          // Apply active filter
          if (request.IsActive.HasValue)
          {
              query = query.Where(u => u.IsActive == request.IsActive.Value);
          }
          
          // Apply sorting
          query = request.SortBy switch
          {
              "Email" => request.SortOrder.ToLower() == "asc" 
                  ? query.OrderBy(u => u.Email.Value) 
                  : query.OrderByDescending(u => u.Email.Value),
              "FirstName" => request.SortOrder.ToLower() == "asc"
                  ? query.OrderBy(u => u.FirstName)
                  : query.OrderByDescending(u => u.FirstName),
              _ => request.SortOrder.ToLower() == "asc"
                  ? query.OrderBy(u => u.CreatedAt)
                  : query.OrderByDescending(u => u.CreatedAt)
          };
          
          // Get total count
          var totalCount = await query.CountAsync(ct);
          
          // Apply pagination
          var users = await query
              .Skip((request.PageNumber - 1) * request.PageSize)
              .Take(request.PageSize)
              .ToListAsync(ct);
          
          // Map to DTOs
          var userDtos = _mapper.Map<List<UserDto>>(users);
          
          var paginatedList = new PaginatedList<UserDto>(
              userDtos,
              totalCount,
              request.PageNumber,
              request.PageSize);
          
          return Result<PaginatedList<UserDto>>.Success(paginatedList);
      }
  }
  ```
- [ ] Create `GetUserById/GetUserByIdQuery.cs`
- [ ] Create `GetUserByIdQueryHandler.cs`
- [ ] Create `GetUserRoles/GetUserRolesQuery.cs`
- [ ] Create `GetUserRolesQueryHandler.cs`

#### Day 5: Testing & Documentation
- [ ] Write unit tests for CreateUserCommandHandler
- [ ] Write unit tests for UpdateUserCommandHandler
- [ ] Write unit tests for DeleteUserCommandHandler
- [ ] Write unit tests for AssignRoleCommandHandler
- [ ] Write unit tests for GetUsersQueryHandler:
  - Test pagination
  - Test search filtering
  - Test sorting
  - Test tenant isolation
- [ ] Write unit tests for GetUserByIdQueryHandler
- [ ] Document users feature in code comments
- [ ] Add example requests/responses in documentation

### Success Criteria
- ✅ All user CRUD commands work correctly
- ✅ User queries return paginated, filtered results
- ✅ Role assignment/removal works
- ✅ Tenant isolation enforced in queries
- ✅ AutoMapper mappings work correctly
- ✅ All unit tests pass
- ✅ Validators enforce business rules

### Risks & Mitigation
- **Risk**: N+1 query problems
  - **Mitigation**: Use .Include() for eager loading
- **Risk**: Large result sets
  - **Mitigation**: Enforce max page size (100)
- **Risk**: Complex filtering logic
  - **Mitigation**: Keep filters simple, add indexes

### Notes
- Always include roles when querying users for complete data
- Test pagination edge cases (empty results, single page)
- Consider caching for frequently accessed users

---

## Phase 9: API Layer & Controllers

**Duration**: 4 days  
**Status**: ⚪ Not Started  
**Priority**: 🟠 High  
**Dependencies**: Phase 8

### Objectives
- Create base API controller
- Implement AuthController with login/register endpoints
- Implement UsersController with CRUD endpoints
- Configure routing and versioning
- Add proper HTTP status codes

### Tasks

#### Day 1: Base Controller & API Structure
- [ ] Create `Presentation/API/Controllers/ApiController.cs`:
  ```csharp
  [ApiController]
  [Route("api/v1/[controller]")]
  [Produces("application/json")]
  public abstract class ApiController : ControllerBase
  {
      private ISender _mediator;
      
      protected ISender Mediator => _mediator ??= 
          HttpContext.RequestServices.GetRequiredService<ISender>();
      
      protected IActionResult HandleResult<T>(Result<T> result)
      {
          if (result.IsSuccess)
          {
              return Ok(new ApiResponse<T>
              {
                  Success = true,
                  Data = result.Value
              });
          }
          
          return BadRequest(new ApiResponse<T>
          {
              Success = false,
              Message = result.Error
          });
      }
      
      protected IActionResult HandleFailure<T>(Result<T> result)
      {
          return result switch
          {
              { IsSuccess: true } => throw new InvalidOperationException(),
              { Error: "Not found" } => NotFound(new { error = result.Error }),
              { Error: "Forbidden" } => Forbid(),
              _ => BadRequest(new { error = result.Error })
          };
      }
  }
  ```
- [ ] Configure API versioning in Program.cs
- [ ] Configure JSON serialization options:
  - Camel case naming
  - Ignore null values
  - Handle reference loops
- [ ] Add CORS configuration

#### Day 2: AuthController
- [ ] Create `Presentation/API/Controllers/AuthController.cs`:
  ```csharp
  public class AuthController : ApiController
  {
      /// <summary>
      /// Login with email and password
      /// </summary>
      /// <param name="command">Login credentials</param>
      /// <returns>Access token and refresh token</returns>
      [HttpPost("login")]
      [AllowAnonymous]
      [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> Login([FromBody] LoginCommand command)
      {
          var result = await Mediator.Send(command);
          return HandleResult(result);
      }
      
      /// <summary>
      /// Register a new user
      /// </summary>
      /// <param name="command">Registration details</param>
      /// <returns>Created user information</returns>
      [HttpPost("register")]
      [AllowAnonymous]
      [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> Register([FromBody] RegisterCommand command)
      {
          var result = await Mediator.Send(command);
          
          if (result.IsFailure)
              return BadRequest(new ApiResponse<object> 
              { 
                  Success = false, 
                  Message = result.Error 
              });
          
          return CreatedAtAction(
              nameof(UsersController.GetUserById),
              "Users",
              new { id = result.Value.UserId },
              new ApiResponse<RegisterResponse> 
              { 
                  Success = true, 
                  Data = result.Value 
              });
      }
      
      /// <summary>
      /// Refresh access token using refresh token
      /// </summary>
      /// <param name="command">Refresh token</param>
      /// <returns>New access token and refresh token</returns>
      [HttpPost("refresh")]
      [AllowAnonymous]
      [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
      {
          var result = await Mediator.Send(command);
          return HandleResult(result);
      }
      
      /// <summary>
      /// Logout current user (revoke refresh token)
      /// </summary>
      [HttpPost("logout")]
      [Authorize]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
      public async Task<IActionResult> Logout()
      {
          // Implement logout logic
          return NoContent();
      }
  }
  ```
- [ ] Test all auth endpoints with Postman/curl
- [ ] Verify JWT tokens are returned correctly

#### Day 3: UsersController
- [ ] Create `Presentation/API/Controllers/UsersController.cs`:
  ```csharp
  [Authorize]
  public class UsersController : ApiController
  {
      /// <summary>
      /// Get paginated list of users
      /// </summary>
      /// <param name="query">Query parameters</param>
      /// <returns>Paginated list of users</returns>
      [HttpGet]
      [ProducesResponseType(typeof(ApiResponse<PaginatedList<UserDto>>), StatusCodes.Status200OK)]
      public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
      {
          var result = await Mediator.Send(query);
          return HandleResult(result);
      }
      
      /// <summary>
      /// Get user by ID
      /// </summary>
      /// <param name="id">User ID</param>
      /// <returns>User details</returns>
      [HttpGet("{id:guid}")]
      [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      public async Task<IActionResult> GetUserById(Guid id)
      {
          var result = await Mediator.Send(new GetUserByIdQuery { Id = id });
          
          if (result.IsFailure)
              return NotFound(new { error = result.Error });
          
          return Ok(new ApiResponse<UserDetailDto> 
          { 
              Success = true, 
              Data = result.Value 
          });
      }
      
      /// <summary>
      /// Create a new user
      /// </summary>
      /// <param name="command">User creation details</param>
      /// <returns>Created user ID</returns>
      [HttpPost]
      [Authorize(Roles = "TenantAdmin,SuperAdmin")]
      [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
      {
          var result = await Mediator.Send(command);
          
          if (result.IsFailure)
              return BadRequest(new { error = result.Error });
          
          return CreatedAtAction(
              nameof(GetUserById),
              new { id = result.Value },
              new ApiResponse<Guid> { Success = true, Data = result.Value });
      }
      
      /// <summary>
      /// Update an existing user
      /// </summary>
      /// <param name="id">User ID</param>
      /// <param name="command">Update details</param>
      /// <returns>No content</returns>
      [HttpPut("{id:guid}")]
      [Authorize(Roles = "TenantAdmin,SuperAdmin")]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
      {
          if (id != command.Id)
              return BadRequest(new { error = "ID mismatch" });
          
          var result = await Mediator.Send(command);
          
          if (result.IsFailure)
              return NotFound(new { error = result.Error });
          
          return NoContent();
      }
      
      /// <summary>
      /// Delete a user (soft delete)
      /// </summary>
      /// <param name="id">User ID</param>
      /// <returns>No content</returns>
      [HttpDelete("{id:guid}")]
      [Authorize(Roles = "TenantAdmin,SuperAdmin")]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
      [ProducesResponseType(StatusCodes.Status404NotFound)]
      public async Task<IActionResult> DeleteUser(Guid id)
      {
          var result = await Mediator.Send(new DeleteUserCommand { Id = id });
          
          if (result.IsFailure)
              return NotFound(new { error = result.Error });
          
          return NoContent();
      }
      
      /// <summary>
      /// Get user roles
      /// </summary>
      /// <param name="id">User ID</param>
      /// <returns>List of roles</returns>
      [HttpGet("{id:guid}/roles")]
      [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
      public async Task<IActionResult> GetUserRoles(Guid id)
      {
          var result = await Mediator.Send(new GetUserRolesQuery { UserId = id });
          return HandleResult(result);
      }
      
      /// <summary>
      /// Assign role to user
      /// </summary>
      /// <param name="id">User ID</param>
      /// <param name="command">Role assignment details</param>
      /// <returns>No content</returns>
      [HttpPost("{id:guid}/roles")]
      [Authorize(Roles = "TenantAdmin,SuperAdmin")]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleCommand command)
      {
          if (id != command.UserId)
              return BadRequest(new { error = "ID mismatch" });
          
          var result = await Mediator.Send(command);
          
          if (result.IsFailure)
              return BadRequest(new { error = result.Error });
          
          return NoContent();
      }
      
      /// <summary>
      /// Remove role from user
      /// </summary>
      /// <param name="id">User ID</param>
      /// <param name="roleId">Role ID</param>
      /// <returns>No content</returns>
      [HttpDelete("{id:guid}/roles/{roleId:guid}")]
      [Authorize(Roles = "TenantAdmin,SuperAdmin")]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
      {
          var result = await Mediator.Send(new RemoveRoleCommand 
          { 
              UserId = id, 
              RoleId = roleId 
          });
          
          if (result.IsFailure)
              return BadRequest(new { error = result.Error });
          
          return NoContent();
      }
  }
  ```
- [ ] Test all user endpoints

#### Day 4: Program.cs Configuration & Testing
- [ ] Configure `Program.cs`:
  ```csharp
  var builder = WebApplication.CreateBuilder(args);
  
  // Add services
  builder.Services.AddControllers();
  builder.Services.AddEndpointsApiExplorer();
  
  // Add application layers
  builder.Services.AddApplicationServices();
  builder.Services.AddInfrastructureServices(builder.Configuration);
  
  // Configure authentication
  builder.Services.AddAuthentication(/* JWT config */);
  builder.Services.AddAuthorization();
  
  // Configure CORS
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowAll", policy =>
      {
          policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
      });
  });
  
  var app = builder.Build();
  
  // Configure middleware pipeline
  app.UseHttpsRedirection();
  app.UseCors("AllowAll");
  app.UseAuthentication();
  app.UseAuthorization();
  
  // Add tenant middleware
  app.UseMiddleware<TenantMiddleware>();
  
  app.MapControllers();
  
  app.Run();
  ```
- [ ] Test complete API flow:
  - Register new user
  - Login
  - Get access token
  - Call protected endpoint with token
  - Refresh token
  - Logout
- [ ] Test tenant isolation:
  - Create users in different tenants
  - Verify cross-tenant access is blocked
- [ ] Document all endpoints with examples

### Success Criteria
- ✅ All API endpoints respond correctly
- ✅ Authentication required for protected endpoints
- ✅ Authorization enforced (roles)
- ✅ Tenant isolation working
- ✅ Proper HTTP status codes returned
- ✅ Error responses are consistent
- ✅ API is RESTful and follows conventions

### Risks & Mitigation
- **Risk**: Inconsistent error responses
  - **Mitigation**: Use base controller HandleResult method
- **Risk**: Missing authorization checks
  - **Mitigation**: Review each endpoint for [Authorize] attribute
- **Risk**: CORS issues
  - **Mitigation**: Configure CORS properly for frontend

### Notes
- Always use [ProducesResponseType] for documentation
- Keep controllers thin, delegate to MediatR
- Use XML comments for Swagger documentation

---

## Phase 10-20 Summary

Due to length constraints, I'll provide summaries for remaining phases:

### Phase 10: Tenants Feature (5 days)
- Create/Update/Delete tenant commands
- Tenant provisioning logic
- Tenant queries and statistics
- TenantsController with SuperAdmin authorization

### Phase 11: Roles & Permissions (5 days)
- Role CRUD commands
- Permission management
- RolesController
- Seeding default roles

### Phase 12: MediatR Behaviors (3 days)
- ValidationBehavior
- LoggingBehavior
- TransactionBehavior
- PerformanceBehavior

### Phase 13: Middleware & Error Handling (3 days)
- ExceptionHandlingMiddleware
- RequestLoggingMiddleware
- Global error handling
- Structured logging with Serilog

### Phase 14: Swagger/OpenAPI (2 days)
- Swagger UI configuration
- JWT bearer authorization in Swagger
- XML documentation
- API examples

### Phase 15: Docker Setup (4 days)
- Dockerfile (multi-stage)
- docker-compose.yml
- PostgreSQL container
- Redis container
- Nginx reverse proxy

### Phase 16: Redis Caching (3 days)
- Redis connection
- Cache service implementation
- Query result caching
- Cache invalidation strategy

### Phase 17: Unit Tests - Domain & Application (5 days)
- Domain entity tests
- Value object tests
- Command handler tests
- Query handler tests
- Validator tests

### Phase 18: Integration Tests (5 days)
- API endpoint tests with TestServer
- Database tests with Testcontainers
- Multi-tenancy isolation tests
- Authentication flow tests

### Phase 19: Documentation (3 days)
- API.md with all endpoints
- AUTHENTICATION.md
- MULTITENANCY.md
- DEPLOYMENT.md
- README updates

### Phase 20: Performance & Security (4 days)
- Query optimization
- Index analysis
- Security headers
- Rate limiting
- Penetration testing

---

## Progress Tracking

### How to Update Status

When starting a phase:
```
Status: 🔵 In Progress
Started: [Date]
Assigned: [Name]
```

When completing a phase:
```
Status: 🟢 Completed
Completed: [Date]
Notes: [Any important notes]
```

When blocked:
```
Status: 🟡 Blocked
Blocker: [Description of blocker]
Action Needed: [What's needed to unblock]
```

### Weekly Review Checklist
- [ ] Review completed phases
- [ ] Update phase statuses
- [ ] Identify blockers
- [ ] Adjust timeline if needed
- [ ] Document lessons learned

---

## Dependencies Graph

```
Phase 1 (Solution Setup)
  ↓
Phase 2 (Domain)
  ↓
Phase 3 (Application Setup)
  ↓
Phase 4 (EF Core)
  ↓
Phase 5 (Multi-Tenancy)
  ↓
Phase 6 (Authentication)
  ↓
Phase 7 (Auth Feature) ────┐
  ↓                         │
Phase 8 (Users Feature) ───┤
  ↓                         │
Phase 9 (API Layer) ────────┤
  ↓                         │
Phase 10 (Tenants) ─────────┤
  ↓                         │
Phase 11 (Roles) ───────────┤
  │                         ↓
  └──────────────→ Phase 12 (Behaviors)
                            ↓
                   Phase 13 (Middleware)
                            ↓
                   Phase 14 (Swagger)
                            ↓
                   Phase 15 (Docker)
                            ↓
                   Phase 16 (Caching)
                            ↓
  ┌────────────────────────┤
  │                        │
  ↓                        ↓
Phase 17 (Unit Tests)  Phase 18 (Integration Tests)
  │                        │
  └──────────┬─────────────┘
             ↓
    Phase 19 (Documentation)
             ↓
    Phase 20 (Performance)
```

---

## Notes & Best Practices

### During Implementation
1. **Commit frequently** - Small, atomic commits
2. **Write tests first** - TDD when possible
3. **Document as you go** - Don't leave it for later
4. **Code review** - All code should be reviewed
5. **Run tests** - Before every commit
6. **Update this plan** - Keep status current

### Code Quality Checks
- [ ] No compiler warnings
- [ ] All tests passing
- [ ] Code coverage > 80%
- [ ] No TODO comments in production code
- [ ] XML documentation on public APIs
- [ ] Follows SOLID principles
- [ ] Follows Clean Code practices

### Before Moving to Next Phase
- [ ] All tasks completed
- [ ] Tests written and passing
- [ ] Documentation updated
- [ ] Code reviewed
- [ ] No blocking issues
- [ ] Success criteria met

---

## Contact & Support

**Project Lead**: [Name]  
**Team**: [Team members]  
**Repository**: [GitHub URL]  
**Documentation**: [Wiki/Docs URL]

---

**Last Updated**: February 18, 2026  
**Version**: 1.0  
**Total Progress**: 0/20 phases completed (0%)
