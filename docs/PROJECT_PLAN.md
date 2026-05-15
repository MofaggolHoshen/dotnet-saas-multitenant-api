# dotnet-saas-multitenant-api - Comprehensive Project Plan

## Project Overview
A scalable multi-tenant SaaS backend built with ASP.NET Core (.NET 10) featuring tenant-based isolation, JWT authentication, role-based authorization, and dynamic external service integrations. Designed with **Clean Architecture**, **CQRS pattern**, **Domain-Driven Design (DDD)**, and production-ready resilience patterns using **MediatR** for command/query separation.

---

## Core Features

### 1. JWT Authentication
- **Token Generation**: Implement JWT token generation with configurable expiration
- **Token Validation**: Middleware for validating JWT tokens on protected endpoints
- **Refresh Tokens**: Support for refresh token mechanism to extend sessions
- **Claims Management**: Store user identity, roles, and tenant information in JWT claims
- **Token Revocation**: Implement blacklist/whitelist strategy for token revocation
- **Security**: Use asymmetric encryption (RS256) for production environments

### 2. Role-Based Authorization System
- **Role Hierarchy**: Define roles (SuperAdmin, TenantAdmin, TenantUser, etc.)
- **Permission Management**: Granular permissions assigned to roles
- **Policy-Based Authorization**: Implement custom authorization policies
- **Role Assignment**: APIs for assigning/removing roles to users
- **Dynamic Roles**: Support for tenant-specific custom roles
- **Role Claims**: Include role information in JWT claims for efficient authorization

### 3. Tenant Isolation
- **Multi-Tenancy Strategy**: 
  - Database per tenant (maximum isolation)
  - Schema per tenant (balanced approach)
  - Shared database with tenant discriminator (cost-effective)
- **Tenant Resolution**: 
  - Header-based tenant identification
  - Subdomain-based resolution
  - Custom route parameter
- **Data Isolation**: Ensure complete data separation between tenants
- **Tenant Context**: Middleware to resolve and inject tenant context
- **Cross-Tenant Prevention**: Guards against accidental cross-tenant data access
- **Tenant Provisioning**: Automated tenant onboarding and database setup

### 4. Entity Framework Core
- **Database Support**: PostgreSQL, SQL Server, MySQL compatibility
- **Migrations**: Automated EF Core migrations per tenant
- **Repository Pattern**: Generic repository with Unit of Work
- **Audit Trails**: Automatic created/updated timestamps and user tracking
- **Soft Deletes**: Implement soft delete pattern with IsDeleted flag
- **Query Filters**: Global query filters for tenant isolation
- **Performance**: 
  - Implement database connection pooling
  - Use compiled queries where appropriate
  - Optimize N+1 query problems with eager loading

### 5. Docker Support
- **Multi-Stage Builds**: Optimize Docker images for production
- **Docker Compose**: 
  - API service
  - Database service (PostgreSQL/SQL Server)
  - Redis for caching/sessions
  - Nginx reverse proxy
- **Environment Configuration**: Support for development, staging, production
- **Health Checks**: Container health monitoring
- **Volume Management**: Persistent data storage
- **Networking**: Isolated container networks
- **Secrets Management**: Secure handling of sensitive configuration

### 6. Swagger/OpenAPI Documentation
- **API Documentation**: Comprehensive Swagger UI with all endpoints
- **Authentication Support**: JWT bearer token input in Swagger
- **Versioning**: API version support (v1, v2, etc.)
- **Examples**: Request/response examples for each endpoint
- **Schemas**: Complete DTO and model documentation
- **Try It Out**: Functional testing directly from Swagger UI
- **Multiple Environments**: Support for dev, staging, prod configurations

---

## Architecture

### Clean Architecture + CQRS Pattern

This project follows **Clean Architecture** principles combined with **CQRS (Command Query Responsibility Segregation)** and **Domain-Driven Design (DDD)** using **MediatR** for request handling.

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│              (API Controllers, Minimal APIs)                 │
│                  ↓ HTTP Requests ↓                          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌──────────────────┐          ┌──────────────────┐        │
│  │   Commands       │          │     Queries      │        │
│  │  (Write Model)   │          │   (Read Model)   │        │
│  ├──────────────────┤          ├──────────────────┤        │
│  │ • CreateUser     │          │ • GetUserById    │        │
│  │ • UpdateTenant   │          │ • GetAllRoles    │        │
│  │ • AssignRole     │          │ • SearchUsers    │        │
│  └────────┬─────────┘          └────────┬─────────┘        │
│           │                              │                  │
│  ┌────────▼──────────────────────────────▼─────────┐       │
│  │          MediatR Request Pipeline                │       │
│  │  • Validation (FluentValidation)                 │       │
│  │  • Logging                                       │       │
│  │  • Transaction Management                        │       │
│  │  • Authorization                                 │       │
│  └────────┬──────────────────────────────┬─────────┘       │
│           │                              │                  │
│  ┌────────▼─────────┐          ┌────────▼─────────┐       │
│  │ Command Handlers │          │  Query Handlers  │       │
│  └──────────────────┘          └──────────────────┘       │
│           │                              │                  │
└───────────┼──────────────────────────────┼──────────────────┘
            │                              │
            ↓                              ↓
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Entities   │  │ Value Objects│  │  Domain Events│     │
│  │ • User       │  │ • Email      │  │ • UserCreated │     │
│  │ • Tenant     │  │ • TenantId   │  │ • RoleAssigned│     │
│  │ • Role       │  │ • Password   │  └───────────────┘     │
│  └──────────────┘  └──────────────┘                        │
│                                                              │
│  ┌──────────────────────────────────────────────┐          │
│  │         Domain Services & Business Rules      │          │
│  │  • Tenant Isolation Rules                     │          │
│  │  • Authorization Policies                     │          │
│  │  • Business Invariants                        │          │
│  └──────────────────────────────────────────────┘          │
│                                                              │
│  ┌──────────────────────────────────────────────┐          │
│  │           Repository Interfaces               │          │
│  │  (Abstractions - no implementation)           │          │
│  └──────────────────────────────────────────────┘          │
└──────────────────────────┬───────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │ EF Core Repos    │  │  External APIs   │                │
│  │ • UserRepository │  │  • Email Service │                │
│  │ • TenantRepo     │  │  • SMS Service   │                │
│  └──────────────────┘  └──────────────────┘                │
│                                                              │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │  Caching (Redis) │  │  Authentication  │                │
│  │  • Query Cache   │  │  • JWT Generator │                │
│  │  • Distributed   │  │  • Token Validator│               │
│  └──────────────────┘  └──────────────────┘                │
│                                                              │
│  ┌──────────────────────────────────────────┐              │
│  │         Persistence (EF Core)             │              │
│  │  • ApplicationDbContext                   │              │
│  │  • TenantDbContext (Multi-tenancy)        │              │
│  │  • Migrations & Seeding                   │              │
│  └──────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────┘
```

### CQRS Benefits
- **Separation of Concerns**: Commands modify state, Queries read state
- **Scalability**: Read and write models can be scaled independently
- **Performance**: Optimize queries without affecting commands
- **Maintainability**: Clear separation makes code easier to understand
- **Flexibility**: Different data models for reads and writes

### Project Structure (Clean Architecture + CQRS)

```
dotnet-saas-multitenant-api/
├── source/
│   ├── Core/
│   │   ├── Domain/                              # Core business logic
│   │   │   ├── Entities/                        # Aggregate roots
│   │   │   │   ├── User.cs
│   │   │   │   ├── Tenant.cs
│   │   │   │   ├── Role.cs
│   │   │   │   ├── Permission.cs
│   │   │   │   └── BaseEntity.cs                # Base with Id, timestamps
│   │   │   ├── ValueObjects/                    # Immutable value objects
│   │   │   │   ├── Email.cs
│   │   │   │   ├── TenantId.cs
│   │   │   │   ├── Password.cs
│   │   │   │   └── SubscriptionTier.cs
│   │   │   ├── Events/                          # Domain events
│   │   │   │   ├── UserCreatedEvent.cs
│   │   │   │   ├── TenantProvisionedEvent.cs
│   │   │   │   ├── RoleAssignedEvent.cs
│   │   │   │   └── IDomainEvent.cs
│   │   │   ├── Repositories/                    # Repository interfaces
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── ITenantRepository.cs
│   │   │   │   ├── IRoleRepository.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Services/                        # Domain services
│   │   │   │   ├── ITenantIsolationService.cs
│   │   │   │   └── IPasswordHashingService.cs
│   │   │   ├── Exceptions/                      # Domain exceptions
│   │   │   │   ├── DomainException.cs
│   │   │   │   ├── TenantNotFoundException.cs
│   │   │   │   └── UserAlreadyExistsException.cs
│   │   │   └── Common/
│   │   │       ├── Result.cs                    # Result pattern
│   │   │       └── Error.cs
│   │   │
│   │   └── Application/                         # Application logic (CQRS)
│   │       ├── Common/
│   │       │   ├── Behaviors/                   # MediatR pipeline behaviors
│   │       │   │   ├── ValidationBehavior.cs
│   │       │   │   ├── LoggingBehavior.cs
│   │       │   │   ├── TransactionBehavior.cs
│   │       │   │   ├── PerformanceBehavior.cs
│   │       │   │   └── AuthorizationBehavior.cs
│   │       │   ├── Interfaces/
│   │       │   │   ├── IApplicationDbContext.cs
│   │       │   │   ├── IDateTime.cs
│   │       │   │   ├── ICurrentUserService.cs
│   │       │   │   └── ITenantContext.cs
│   │       │   ├── Mappings/
│   │       │   │   └── MappingProfile.cs        # AutoMapper profiles
│   │       │   └── Models/
│   │       │       ├── PaginatedList.cs
│   │       │       └── ApiResponse.cs
│   │       │
│   │       ├── Features/                        # Feature folders (CQRS)
│   │       │   ├── Auth/
│   │       │   │   ├── Commands/
│   │       │   │   │   ├── Login/
│   │       │   │   │   │   ├── LoginCommand.cs
│   │       │   │   │   │   ├── LoginCommandHandler.cs
│   │       │   │   │   │   ├── LoginCommandValidator.cs
│   │       │   │   │   │   └── LoginResponse.cs
│   │       │   │   │   ├── Register/
│   │       │   │   │   │   ├── RegisterCommand.cs
│   │       │   │   │   │   ├── RegisterCommandHandler.cs
│   │       │   │   │   │   └── RegisterCommandValidator.cs
│   │       │   │   │   └── RefreshToken/
│   │       │   │   │       ├── RefreshTokenCommand.cs
│   │       │   │   │       └── RefreshTokenCommandHandler.cs
│   │       │   │   └── Queries/
│   │       │   │       └── ValidateToken/
│   │       │   │           ├── ValidateTokenQuery.cs
│   │       │   │           └── ValidateTokenQueryHandler.cs
│   │       │   │
│   │       │   ├── Users/
│   │       │   │   ├── Commands/
│   │       │   │   │   ├── CreateUser/
│   │       │   │   │   │   ├── CreateUserCommand.cs
│   │       │   │   │   │   ├── CreateUserCommandHandler.cs
│   │       │   │   │   │   └── CreateUserCommandValidator.cs
│   │       │   │   │   ├── UpdateUser/
│   │       │   │   │   │   ├── UpdateUserCommand.cs
│   │       │   │   │   │   └── UpdateUserCommandHandler.cs
│   │       │   │   │   ├── DeleteUser/
│   │       │   │   │   │   ├── DeleteUserCommand.cs
│   │       │   │   │   │   └── DeleteUserCommandHandler.cs
│   │       │   │   │   └── AssignRole/
│   │       │   │   │       ├── AssignRoleCommand.cs
│   │       │   │   │       └── AssignRoleCommandHandler.cs
│   │       │   │   ├── Queries/
│   │       │   │   │   ├── GetUsers/
│   │       │   │   │   │   ├── GetUsersQuery.cs
│   │       │   │   │   │   ├── GetUsersQueryHandler.cs
│   │       │   │   │   │   └── UserDto.cs
│   │       │   │   │   ├── GetUserById/
│   │       │   │   │   │   ├── GetUserByIdQuery.cs
│   │       │   │   │   │   ├── GetUserByIdQueryHandler.cs
│   │       │   │   │   │   └── UserDetailDto.cs
│   │       │   │   │   └── GetUserRoles/
│   │       │   │   │       ├── GetUserRolesQuery.cs
│   │       │   │   │       └── GetUserRolesQueryHandler.cs
│   │       │   │   └── EventHandlers/
│   │       │   │       └── UserCreatedEventHandler.cs
│   │       │   │
│   │       │   ├── Tenants/
│   │       │   │   ├── Commands/
│   │       │   │   │   ├── CreateTenant/
│   │       │   │   │   │   ├── CreateTenantCommand.cs
│   │       │   │   │   │   └── CreateTenantCommandHandler.cs
│   │       │   │   │   ├── UpdateTenant/
│   │       │   │   │   ├── DeleteTenant/
│   │       │   │   │   └── ProvisionTenant/
│   │       │   │   │       ├── ProvisionTenantCommand.cs
│   │       │   │   │       └── ProvisionTenantCommandHandler.cs
│   │       │   │   ├── Queries/
│   │       │   │   │   ├── GetTenants/
│   │       │   │   │   ├── GetTenantById/
│   │       │   │   │   └── GetTenantStats/
│   │       │   │   └── EventHandlers/
│   │       │   │       └── TenantProvisionedEventHandler.cs
│   │       │   │
│   │       │   └── Roles/
│   │       │       ├── Commands/
│   │       │       │   ├── CreateRole/
│   │       │       │   ├── UpdateRole/
│   │       │       │   ├── DeleteRole/
│   │       │       │   └── AssignPermission/
│   │       │       └── Queries/
│   │       │           ├── GetRoles/
│   │       │           ├── GetRoleById/
│   │       │           └── GetRolePermissions/
│   │       │
│   │       └── DependencyInjection.cs          # Service registration
│   │
│   ├── Infrastructure/                          # External concerns
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── TenantDbContext.cs
│   │   │   ├── Configurations/                  # EF Core configurations
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── TenantConfiguration.cs
│   │   │   │   └── RoleConfiguration.cs
│   │   │   ├── Interceptors/
│   │   │   │   ├── AuditableEntityInterceptor.cs
│   │   │   │   └── SoftDeleteInterceptor.cs
│   │   │   ├── Repositories/                    # Repository implementations
│   │   │   │   ├── UserRepository.cs
│   │   │   │   ├── TenantRepository.cs
│   │   │   │   ├── RoleRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   ├── Migrations/
│   │   │   └── Seeds/
│   │   │       └── DefaultDataSeeder.cs
│   │   │
│   │   ├── Identity/
│   │   │   ├── JwtTokenGenerator.cs
│   │   │   ├── JwtSettings.cs
│   │   │   ├── PasswordHasher.cs
│   │   │   └── CurrentUserService.cs
│   │   │
│   │   ├── Multitenancy/
│   │   │   ├── TenantContext.cs
│   │   │   ├── TenantResolver.cs
│   │   │   ├── TenantMiddleware.cs
│   │   │   └── TenantService.cs
│   │   │
│   │   ├── Caching/
│   │   │   ├── RedisCacheService.cs
│   │   │   └── CacheSettings.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── DateTimeService.cs
│   │   │   ├── EmailService.cs
│   │   │   └── SmsService.cs
│   │   │
│   │   └── DependencyInjection.cs
│   │
│   ├── Presentation/
│   │   └── API/                                 # REST API
│   │       ├── Controllers/
│   │       │   ├── ApiController.cs             # Base controller
│   │       │   ├── AuthController.cs
│   │       │   ├── UsersController.cs
│   │       │   ├── TenantsController.cs
│   │       │   └── RolesController.cs
│   │       ├── Middleware/
│   │       │   ├── ExceptionHandlingMiddleware.cs
│   │       │   ├── TenantResolutionMiddleware.cs
│   │       │   └── RequestLoggingMiddleware.cs
│   │       ├── Filters/
│   │       │   ├── ApiExceptionFilterAttribute.cs
│   │       │   └── ValidateModelStateAttribute.cs
│   │       ├── Extensions/
│   │       │   └── ServiceCollectionExtensions.cs
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── appsettings.Development.json
│   │
│   └── Tests/
│       ├── Domain.UnitTests/
│       │   ├── Entities/
│       │   └── ValueObjects/
│       ├── Application.UnitTests/
│       │   ├── Commands/
│       │   └── Queries/
│       ├── Application.IntegrationTests/
│       │   ├── Commands/
│       │   ├── Queries/
│       │   └── TestFixtures/
│       ├── Infrastructure.IntegrationTests/
│       │   └── Repositories/
│       └── API.IntegrationTests/
│           └── Controllers/
│
├── docker/
│   ├── Dockerfile
│   ├── docker-compose.yml
│   ├── docker-compose.dev.yml
│   └── nginx.conf
│
└── docs/
    ├── PROJECT_PLAN.md
    ├── CLEAN_ARCHITECTURE.md
    ├── CQRS_PATTERN.md
    ├── API.md
    ├── AUTHENTICATION.md
    ├── DEPLOYMENT.md
    └── MULTITENANCY.md
```

---

## Clean Code & Design Principles

### SOLID Principles

#### 1. Single Responsibility Principle (SRP)
- Each class/method has one reason to change
- Separate commands from queries (CQRS)
- Domain entities contain only business logic
- Infrastructure handles technical concerns

**Example:**
```csharp
// ✅ GOOD - Single responsibility
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Only handles user creation logic
    }
}

// ❌ BAD - Multiple responsibilities
public class UserService
{
    public void CreateUser() { }
    public void SendEmail() { }
    public void LogActivity() { }
    public void UpdateDatabase() { }
}
```

#### 2. Open/Closed Principle (OCP)
- Open for extension, closed for modification
- Use strategies, decorators, and MediatR behaviors
- Extend functionality through new handlers, not modifying existing ones

**Example:**
```csharp
// ✅ GOOD - Add new behavior without modifying existing code
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Pluggable validation pipeline
}

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Pluggable logging pipeline
}
```

#### 3. Liskov Substitution Principle (LSP)
- Derived classes must be substitutable for base classes
- Repository implementations interchangeable
- Use interfaces for abstractions

**Example:**
```csharp
// ✅ GOOD - Any IUserRepository can be substituted
public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
}

public class UserRepository : IUserRepository { }
public class CachedUserRepository : IUserRepository { }
```

#### 4. Interface Segregation Principle (ISP)
- Many specific interfaces > one general interface
- Clients shouldn't depend on methods they don't use

**Example:**
```csharp
// ✅ GOOD - Segregated interfaces
public interface IReadRepository<T> 
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
}

public interface IWriteRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

// ❌ BAD - Bloated interface
public interface IRepository<T>
{
    // 20+ methods that most clients don't need
}
```

#### 5. Dependency Inversion Principle (DIP)
- Depend on abstractions, not concretions
- Domain doesn't reference Infrastructure
- Use dependency injection

**Example:**
```csharp
// ✅ GOOD - Depends on abstraction
public class CreateUserCommandHandler
{
    private readonly IUserRepository _repository; // Interface
    private readonly IPasswordHasher _hasher;     // Interface
    
    public CreateUserCommandHandler(IUserRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }
}

// ❌ BAD - Depends on concrete implementation
public class UserService
{
    private readonly SqlUserRepository _repository; // Concrete class
}
```

### Domain-Driven Design (DDD) Patterns

#### 1. Entities
Rich domain models with identity and business logic
```csharp
public class User : BaseEntity
{
    public TenantId TenantId { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    
    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    
    // Factory method
    public static User Create(TenantId tenantId, Email email, string passwordHash)
    {
        // Business rule: Email must be valid
        if (email == null) throw new ArgumentNullException(nameof(email));
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        user.AddDomainEvent(new UserCreatedEvent(user.Id, email.Value));
        return user;
    }
    
    // Business logic method
    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
            throw new DomainException("User already has this role");
            
        _roles.Add(role);
        AddDomainEvent(new RoleAssignedEvent(Id, role.Id));
    }
    
    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new UserDeactivatedEvent(Id));
    }
}
```

#### 2. Value Objects
Immutable objects defined by their attributes
```csharp
public class Email : ValueObject
{
    public string Value { get; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure("Email cannot be empty");
            
        if (!IsValidEmail(email))
            return Result<Email>.Failure("Email format is invalid");
            
        return Result<Email>.Success(new Email(email.ToLowerInvariant()));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}

public class TenantId : ValueObject
{
    public Guid Value { get; }
    
    private TenantId(Guid value)
    {
        Value = value;
    }
    
    public static TenantId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(value));
            
        return new TenantId(value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

#### 3. Aggregates
Cluster of entities with clear boundaries
```csharp
// User is the aggregate root
public class User : AggregateRoot
{
    // Aggregate root controls access to child entities
    private readonly List<UserRole> _userRoles = new();
    
    // Encapsulation - no public setter
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    
    // All operations go through aggregate root
    public void AssignRole(Role role)
    {
        var userRole = new UserRole(Id, role.Id);
        _userRoles.Add(userRole);
        
        AddDomainEvent(new RoleAssignedEvent(Id, role.Id));
    }
}
```

#### 4. Domain Events
Capture something significant that happened in the domain
```csharp
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public record UserCreatedEvent(Guid UserId, string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TenantProvisionedEvent(Guid TenantId, string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Domain event handler
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    
    public async Task Handle(UserCreatedEvent notification, CancellationToken ct)
    {
        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(notification.Email);
    }
}
```

#### 5. Repository Pattern
Abstractions for data access
```csharp
public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<IEnumerable<User>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

// Implementation in Infrastructure layer
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<User> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}
```

#### 6. Unit of Work Pattern
Maintains list of objects and coordinates changes
```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITenantRepository Tenants { get; }
    IRoleRepository Roles { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

### Clean Code Practices

#### 1. Meaningful Names
```csharp
// ✅ GOOD
public class CreateUserCommand { }
public class UserCreatedEvent { }
public async Task<Result<User>> GetUserByIdAsync(Guid userId) { }

// ❌ BAD
public class Cmd1 { }
public class Event { }
public async Task<object> Get(Guid id) { }
```

#### 2. Small Functions
```csharp
// ✅ GOOD - Does one thing
public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
{
    var emailResult = Email.Create(request.Email);
    if (emailResult.IsFailure)
        return Result<Guid>.Failure(emailResult.Error);
        
    var user = User.Create(request.TenantId, emailResult.Value, request.PasswordHash);
    
    await _repository.AddAsync(user, ct);
    await _unitOfWork.SaveChangesAsync(ct);
    
    return Result<Guid>.Success(user.Id);
}

// ❌ BAD - Does too many things
public async Task DoEverything() 
{
    // 200 lines of mixed concerns
}
```

#### 3. Avoid Magic Numbers/Strings
```csharp
// ✅ GOOD
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string User = "User";
}

[Authorize(Roles = Roles.SuperAdmin)]
public async Task<IActionResult> GetAllTenants() { }

// ❌ BAD
[Authorize(Roles = "SuperAdmin")]
public async Task<IActionResult> GetAllTenants() { }
```

#### 4. Result Pattern (No Exceptions for Flow Control)
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

// Usage
public async Task<Result<User>> GetUserAsync(Guid id)
{
    var user = await _repository.GetByIdAsync(id);
    
    if (user == null)
        return Result<User>.Failure("User not found");
        
    return Result<User>.Success(user);
}
```

#### 5. Guard Clauses
```csharp
// ✅ GOOD - Early returns
public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
{
    if (request == null)
        return Result<Guid>.Failure("Request cannot be null");
        
    if (string.IsNullOrEmpty(request.Email))
        return Result<Guid>.Failure("Email is required");
        
    var existingUser = await _repository.GetByEmailAsync(request.Email, ct);
    if (existingUser != null)
        return Result<Guid>.Failure("User already exists");
        
    // Main logic here
}

// ❌ BAD - Nested ifs
public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
{
    if (request != null)
    {
        if (!string.IsNullOrEmpty(request.Email))
        {
            var existingUser = await _repository.GetByEmailAsync(request.Email, ct);
            if (existingUser == null)
            {
                // Main logic deeply nested
            }
        }
    }
}
```

#### 6. Dependency Injection Over Service Locator
```csharp
// ✅ GOOD - Constructor injection
public class CreateUserCommandHandler
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _hasher;
    
    public CreateUserCommandHandler(IUserRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }
}

// ❌ BAD - Service locator anti-pattern
public class CreateUserCommandHandler
{
    public async Task Handle()
    {
        var repository = ServiceLocator.Get<IUserRepository>();
        var hasher = ServiceLocator.Get<IPasswordHasher>();
    }
}
```

### Code Organization Standards

1. **Feature Folders**: Organize by features, not technical layers
2. **Vertical Slices**: Each feature is self-contained (Commands, Queries, DTOs, Validators)
3. **Explicit Dependencies**: All dependencies injected through constructor
4. **Immutability**: Prefer readonly fields, records, and value objects
5. **Async/Await**: Use async methods consistently for I/O operations
6. **Cancellation Tokens**: Support cancellation in all async methods
7. **Configuration Over Convention**: Be explicit about mappings and configurations
8. **Validation**: Use FluentValidation for all input validation
9. **Error Handling**: Use Result pattern, domain exceptions for business rules
10. **Testing**: Write tests for all business logic (commands, queries, domain logic)

---

## CQRS Pattern Implementation

### Command Example (Write Operation)

#### 1. Command Definition
```csharp
public record CreateUserCommand : IRequest<Result<Guid>>
{
    public Guid TenantId { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
}
```

#### 2. Command Validator
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain number");
            
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .MaximumLength(50);
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .MaximumLength(50);
    }
}
```

#### 3. Command Handler
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    
    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating user with email {Email}", request.Email);
        
        // Create value object
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result<Guid>.Failure(emailResult.Error);
        
        // Check if user exists
        var existingUser = await _userRepository.GetByEmailAsync(emailResult.Value, ct);
        if (existingUser != null)
            return Result<Guid>.Failure("User with this email already exists");
        
        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);
        
        // Create domain entity
        var tenantId = TenantId.Create(request.TenantId);
        var user = User.Create(tenantId, emailResult.Value, passwordHash);
        user.SetName(request.FirstName, request.LastName);
        
        // Save to database
        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("User created successfully with Id {UserId}", user.Id);
        
        return Result<Guid>.Success(user.Id);
    }
}
```

#### 4. Controller Action
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="command">User creation details</param>
    /// <returns>Created user ID</returns>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
            
        return CreatedAtAction(nameof(GetUserById), new { id = result.Value }, result.Value);
    }
}
```

### Query Example (Read Operation)

#### 1. Query Definition
```csharp
public record GetUsersQuery : IRequest<Result<PaginatedList<UserDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SearchTerm { get; init; }
    public bool? IsActive { get; init; }
}
```

#### 2. Query DTO
```csharp
public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<string> Roles { get; init; } = new();
}
```

#### 3. Query Handler
```csharp
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PaginatedList<UserDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetUsersQueryHandler> _logger;
    
    public GetUsersQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ITenantContext tenantContext,
        ILogger<GetUsersQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _logger = logger;
    }
    
    public async Task<Result<PaginatedList<UserDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Fetching users for tenant {TenantId}", _tenantContext.TenantId);
        
        var query = _context.Users
            .Include(u => u.Roles)
            .AsQueryable();
        
        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => 
                u.Email.Value.Contains(request.SearchTerm) ||
                u.FirstName.Contains(request.SearchTerm) ||
                u.LastName.Contains(request.SearchTerm));
        }
        
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }
        
        // Get total count
        var totalCount = await query.CountAsync(ct);
        
        // Apply pagination
        var users = await query
            .OrderBy(u => u.CreatedAt)
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

#### 4. Controller Action
```csharp
/// <summary>
/// Gets paginated list of users
/// </summary>
/// <param name="query">Query parameters</param>
/// <returns>Paginated list of users</returns>
[HttpGet]
[Authorize]
[ProducesResponseType(typeof(PaginatedList<UserDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
{
    var result = await _mediator.Send(query);
    
    if (result.IsFailure)
        return BadRequest(new { error = result.Error });
        
    return Ok(result.Value);
}
```

### MediatR Pipeline Behaviors

#### 1. Validation Behavior
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();
        
        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        if (failures.Any())
        {
            _logger.LogWarning("Validation failed for {RequestType}: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => f.ErrorMessage)));
                
            throw new ValidationException(failures);
        }
        
        return await next();
    }
}
```

#### 2. Logging Behavior
```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    
    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";
        
        _logger.LogInformation("Handling {RequestName} by User {UserId}", requestName, userId);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            
            stopwatch.Stop();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

#### 3. Transaction Behavior
```csharp
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    
    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only wrap commands in transaction, not queries
        if (request is IQuery<TResponse>)
            return await next();
        
        _logger.LogInformation("Beginning transaction for {RequestType}", typeof(TRequest).Name);
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var response = await next();
            
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Transaction committed for {RequestType}", typeof(TRequest).Name);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction rollback for {RequestType}", typeof(TRequest).Name);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
```

### Marker Interfaces for CQRS

```csharp
// Marker interface for queries (read-only operations)
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

// Marker interface for commands (write operations)
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

// Usage
public record GetUserByIdQuery(Guid Id) : IQuery<Result<UserDetailDto>>;
public record CreateUserCommand : ICommand<Result<Guid>> { /* props */ }
```

---

## Implementation Roadmap

### Phase 1: Foundation & Domain Layer (Week 1-2)
- [ ] Set up solution structure with Clean Architecture + CQRS
- [ ] Create Domain project with entities (User, Tenant, Role, Permission)
- [ ] Implement Value Objects (Email, TenantId, Password, SubscriptionTier)
- [ ] Create Domain Events (UserCreatedEvent, TenantProvisionedEvent, etc.)
- [ ] Define Repository interfaces (IUserRepository, ITenantRepository, IUnitOfWork)
- [ ] Implement Domain Services (ITenantIsolationService, IPasswordHashingService)
- [ ] Create Domain Exceptions (DomainException, TenantNotFoundException)
- [ ] Implement Result pattern for error handling
- [ ] Add base entity with audit fields (CreatedAt, UpdatedAt, CreatedBy)

### Phase 2: Application Layer with CQRS (Week 3-4)
- [ ] Set up Application project with MediatR
- [ ] Create MediatR pipeline behaviors:
  - [ ] ValidationBehavior (with FluentValidation)
  - [ ] LoggingBehavior
  - [ ] TransactionBehavior
  - [ ] PerformanceBehavior
  - [ ] AuthorizationBehavior
- [ ] Implement Auth feature:
  - [ ] LoginCommand and LoginCommandHandler
  - [ ] RegisterCommand and RegisterCommandHandler
  - [ ] RefreshTokenCommand and RefreshTokenCommandHandler
  - [ ] FluentValidation validators for each command
- [ ] Implement User feature:
  - [ ] CreateUserCommand and handler
  - [ ] UpdateUserCommand and handler
  - [ ] DeleteUserCommand and handler
  - [ ] GetUsersQuery and handler (with pagination)
  - [ ] GetUserByIdQuery and handler
- [ ] Set up AutoMapper profiles for DTOs
- [ ] Create common interfaces (IApplicationDbContext, ICurrentUserService)

### Phase 3: Infrastructure & Multi-Tenancy (Week 5-6)
- [ ] Set up Infrastructure project with EF Core
- [ ] Configure ApplicationDbContext and TenantDbContext
- [ ] Implement EF Core configurations for entities
- [ ] Create Repository implementations (UserRepository, TenantRepository, etc.)
- [ ] Implement UnitOfWork pattern
- [ ] Create EF Core interceptors:
  - [ ] AuditableEntityInterceptor (auto timestamps)
  - [ ] SoftDeleteInterceptor (IsDeleted flag)
- [ ] Implement multi-tenancy:
  - [ ] TenantContext service
  - [ ] TenantResolver (header/subdomain based)
  - [ ] TenantMiddleware
  - [ ] Global query filters for tenant isolation
- [ ] Set up initial database migrations
- [ ] Implement JWT token generation and validation
- [ ] Create PasswordHasher service with BCrypt
- [ ] Configure PostgreSQL database connection

### Phase 4: API Layer & Remaining Features (Week 7-8)
- [ ] Create API project with controllers
- [ ] Implement base ApiController with MediatR
- [ ] Create AuthController (Login, Register, RefreshToken)
- [ ] Create UsersController (CRUD + role assignment)
- [ ] Create TenantsController (CRUD + provisioning)
- [ ] Create RolesController (CRUD + permissions)
- [ ] Implement Tenant features (CQRS):
  - [ ] CreateTenantCommand and handler
  - [ ] ProvisionTenantCommand and handler
  - [ ] GetTenantsQuery and handler
  - [ ] GetTenantStatsQuery and handler
- [ ] Implement Role features (CQRS):
  - [ ] CreateRoleCommand and handler
  - [ ] AssignPermissionCommand and handler
  - [ ] GetRolesQuery and handler
  - [ ] GetRolePermissionsQuery and handler
- [ ] Add exception handling middleware
- [ ] Implement request logging middleware
- [ ] Add validation filters

### Phase 5: Docker & DevOps (Week 9-10)
- [ ] Create multi-stage Dockerfile
- [ ] Set up docker-compose for local development
- [ ] Configure Docker networking
- [ ] Add health check endpoints
- [ ] Set up Redis for caching
- [ ] Configure Nginx reverse proxy
- [ ] Create environment-specific configurations
- [ ] Document Docker setup

### Phase 6: Testing, Documentation & Polish (Week 11-12)
- [ ] Configure Swagger/OpenAPI with JWT support
- [ ] Add XML documentation comments to all public APIs
- [ ] Create API examples in Swagger
- [ ] Write unit tests:
  - [ ] Domain entity tests
  - [ ] Command handler tests
  - [ ] Query handler tests
  - [ ] Validator tests
- [ ] Write integration tests:
  - [ ] API endpoint tests
  - [ ] Repository tests
  - [ ] Multi-tenancy isolation tests
- [ ] Create documentation:
  - [ ] CLEAN_ARCHITECTURE.md
  - [ ] CQRS_PATTERN.md
  - [ ] API.md with all endpoints
  - [ ] DEPLOYMENT.md
- [ ] Performance testing and optimization
- [ ] Security audit
- [ ] Code quality checks (SonarQube, etc.)

---

## Technical Stack

### Core Technologies
- **.NET 10**: Latest LTS version
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for database operations
- **PostgreSQL**: Primary database (can be swapped)
- **Redis**: Caching and session management

### Authentication & Security
- **JWT Bearer**: Token-based authentication
- **BCrypt/PBKDF2**: Password hashing
- **RS256**: Asymmetric token signing
- **HTTPS**: Enforced SSL/TLS

### Libraries & Tools
- **MediatR**: CQRS and mediator pattern implementation
- **FluentValidation**: Request validation (integrated with MediatR)
- **AutoMapper**: Object-to-object mapping for DTOs
- **Polly**: Resilience and retry policies
- **Serilog**: Structured logging with enrichers
- **Swashbuckle**: Swagger/OpenAPI generation
- **Scrutor**: Assembly scanning and decoration
- **xUnit**: Unit testing framework
- **FluentAssertions**: Fluent test assertions
- **Moq**: Mocking framework
- **Bogus**: Fake data generation for tests
- **Testcontainers**: Integration testing with Docker containers

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Multi-container orchestration
- **GitHub Actions**: CI/CD pipelines
- **Nginx**: Reverse proxy

---

## Database Schema

### Core Tables

#### Users
```sql
- Id (UUID, PK)
- TenantId (UUID, FK)
- Email (string, unique per tenant)
- PasswordHash (string)
- FirstName (string)
- LastName (string)
- IsActive (bool)
- EmailConfirmed (bool)
- CreatedAt (datetime)
- UpdatedAt (datetime)
- LastLoginAt (datetime)
```

#### Tenants
```sql
- Id (UUID, PK)
- Name (string)
- Subdomain (string, unique)
- ConnectionString (string, encrypted)
- IsActive (bool)
- SubscriptionTier (enum)
- CreatedAt (datetime)
- UpdatedAt (datetime)
```

#### Roles
```sql
- Id (UUID, PK)
- TenantId (UUID, FK, nullable for system roles)
- Name (string)
- Description (string)
- IsSystemRole (bool)
- CreatedAt (datetime)
```

#### Permissions
```sql
- Id (UUID, PK)
- Name (string)
- Description (string)
- Resource (string)
- Action (string)
```

#### UserRoles (Many-to-Many)
```sql
- UserId (UUID, FK)
- RoleId (UUID, FK)
- AssignedAt (datetime)
```

#### RolePermissions (Many-to-Many)
```sql
- RoleId (UUID, FK)
- PermissionId (UUID, FK)
```

---

## API Endpoints

### Authentication
```
POST   /api/v1/auth/register          - Register new user
POST   /api/v1/auth/login             - Login user
POST   /api/v1/auth/refresh           - Refresh access token
POST   /api/v1/auth/logout            - Logout user
POST   /api/v1/auth/forgot-password   - Request password reset
POST   /api/v1/auth/reset-password    - Reset password
GET    /api/v1/auth/verify-email      - Verify email address
```

### Users
```
GET    /api/v1/users                  - Get all users (tenant-scoped)
GET    /api/v1/users/{id}             - Get user by ID
POST   /api/v1/users                  - Create new user
PUT    /api/v1/users/{id}             - Update user
DELETE /api/v1/users/{id}             - Delete user
GET    /api/v1/users/{id}/roles       - Get user roles
POST   /api/v1/users/{id}/roles       - Assign role to user
DELETE /api/v1/users/{id}/roles/{roleId} - Remove role from user
```

### Tenants (SuperAdmin only)
```
GET    /api/v1/tenants                - Get all tenants
GET    /api/v1/tenants/{id}           - Get tenant by ID
POST   /api/v1/tenants                - Create new tenant
PUT    /api/v1/tenants/{id}           - Update tenant
DELETE /api/v1/tenants/{id}           - Delete tenant
POST   /api/v1/tenants/{id}/provision - Provision tenant database
```

### Roles
```
GET    /api/v1/roles                  - Get all roles (tenant-scoped)
GET    /api/v1/roles/{id}             - Get role by ID
POST   /api/v1/roles                  - Create new role
PUT    /api/v1/roles/{id}             - Update role
DELETE /api/v1/roles/{id}             - Delete role
GET    /api/v1/roles/{id}/permissions - Get role permissions
POST   /api/v1/roles/{id}/permissions - Assign permission to role
DELETE /api/v1/roles/{id}/permissions/{permissionId} - Remove permission
```

### Health & Status
```
GET    /health                        - Health check endpoint
GET    /api/v1/status                 - API status and version
```

---

## Security Considerations

### Authentication Security
- Strong password requirements (min 8 chars, complexity rules)
- Password hashing with BCrypt (cost factor 12+)
- JWT token expiration (15-30 minutes for access, 7 days for refresh)
- Secure token storage guidelines
- Rate limiting on authentication endpoints
- Account lockout after failed attempts

### Authorization Security
- Principle of least privilege
- Role-based access control (RBAC)
- Tenant-based data isolation
- Permission checks at service layer
- Audit logging for sensitive operations

### Data Security
- Encryption at rest for sensitive data
- Encryption in transit (HTTPS only)
- SQL injection prevention (parameterized queries)
- XSS prevention (input sanitization)
- CSRF protection
- CORS configuration

### Infrastructure Security
- Environment variable management
- Secrets stored in secure vaults (Azure Key Vault, AWS Secrets Manager)
- Regular dependency updates
- Security headers (HSTS, Content Security Policy)
- Rate limiting and throttling

---

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "env:JWT_SECRET",
    "Issuer": "dotnet-saas-api",
    "Audience": "dotnet-saas-client",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "env:DATABASE_URL"
  },
  "Multitenancy": {
    "Strategy": "SharedDatabase",
    "TenantResolution": "Header"
  },
  "Redis": {
    "ConnectionString": "env:REDIS_URL"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

---

## Testing Strategy

### Unit Tests
- Domain entity logic
- Service layer business rules
- Validators
- Mappers
- Utilities

### Integration Tests
- API endpoint testing
- Database operations
- Authentication flows
- Authorization policies
- Multi-tenancy isolation

### Performance Tests
- Load testing API endpoints
- Database query optimization
- Concurrent user scenarios
- Memory leak detection

---

## Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

### Environment Variables
```bash
DATABASE_URL=postgresql://user:pass@db:5432/saasdb
JWT_SECRET=your-secret-key-min-32-chars
REDIS_URL=redis://redis:6379
ASPNETCORE_ENVIRONMENT=Production
```

### Production Checklist
- [ ] Configure HTTPS/SSL certificates
- [ ] Set up database backups
- [ ] Configure logging and monitoring
- [ ] Set up health checks
- [ ] Configure auto-scaling
- [ ] Implement rate limiting
- [ ] Set up CDN for static assets
- [ ] Configure secrets management
- [ ] Enable CORS for allowed origins
- [ ] Set up CI/CD pipelines

---

## Monitoring & Logging

### Logging
- **Serilog**: Structured logging
- **Log Levels**: Debug, Info, Warning, Error, Critical
- **Log Sinks**: Console, File, Database, External (Seq, ELK)
- **Context Enrichment**: Tenant ID, User ID, Request ID

### Monitoring
- **Health Checks**: Database, Redis, External services
- **Metrics**: Request count, response times, error rates
- **APM**: Application Performance Monitoring (Application Insights, Datadog)
- **Alerts**: Critical errors, performance degradation, downtime

---

## Future Enhancements

### Phase 7: Advanced Features
- [ ] Multi-factor authentication (MFA)
- [ ] OAuth2/OpenID Connect integration
- [ ] Real-time notifications (SignalR)
- [ ] Audit trail dashboard
- [ ] Advanced analytics and reporting
- [ ] API rate limiting per tenant
- [ ] Tenant-specific theming/branding
- [ ] Webhook system for integrations
- [ ] GraphQL API option
- [ ] Mobile app API support

### Scalability Improvements
- [ ] Implement CQRS pattern with MediatR
- [ ] Add read replicas for databases
- [ ] Implement event sourcing
- [ ] Add message queue (RabbitMQ/Azure Service Bus)
- [ ] Implement distributed caching strategy
- [ ] Add API gateway (Ocelot/YARP)
- [ ] Kubernetes deployment manifests
- [ ] Horizontal pod autoscaling

---

## Resources & References

### Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [JWT.io](https://jwt.io)
- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

### Sample Projects
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [Clean Architecture Solution Template](https://github.com/jasontaylordev/CleanArchitecture)

---

## Support & Contribution

### Getting Started
1. Clone the repository
2. Install .NET 10 SDK
3. Run `docker-compose up` to start dependencies
4. Run database migrations
5. Start the API
6. Access Swagger at `http://localhost:5000/swagger`

### Contributing
- Follow Clean Architecture principles
- Write unit tests for new features
- Update documentation
- Follow C# coding conventions
- Create feature branches
- Submit pull requests for review

---

## License
[Specify your license here]

## Contact
[Your contact information]

---

**Last Updated**: February 16, 2026  
**Version**: 1.0.0  
**Status**: Planning Phase
