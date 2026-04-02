# Domain Exceptions & Repository Interfaces - Deep Dive

## 📖 Table of Contents
- [Domain Exceptions](#domain-exceptions)
- [Repository Interfaces](#repository-interfaces)
- [Domain Services](#domain-services)
- [Integration Patterns](#integration-patterns)
- [Best Practices](#best-practices)

---

## Domain Exceptions

### What are Domain Exceptions?

**Domain Exceptions** are exceptions that represent violations of **business rules** or **domain invariants**. They:

- Inherit from a common `DomainException` base class
- Represent **expected exceptional cases** in the domain
- Are **meaningful** to the business
- Should be **caught and handled** appropriately

### Exception Hierarchy

```mermaid
classDiagram
    class Exception {
        <<.NET>>
    }

    class DomainException {
        <<abstract>>
        #DomainException(message)
        #DomainException(message, innerException)
    }

    class TenantNotFoundException {
        +Guid TenantId
        +TenantNotFoundException(tenantId)
    }

    class UserNotFoundException {
        +Guid UserId
        +UserNotFoundException(userId)
    }

    class UserAlreadyExistsException {
        +string Email
        +UserAlreadyExistsException(email)
    }

    class DomainInvalidOperationException {
        +DomainInvalidOperationException(message)
        +DomainInvalidOperationException(message, inner)
    }

    Exception <|-- DomainException
    DomainException <|-- TenantNotFoundException
    DomainException <|-- UserNotFoundException
    DomainException <|-- UserAlreadyExistsException
    DomainException <|-- DomainInvalidOperationException

    style Exception fill:#e1e1e1
    style DomainException fill:#e1f5ff
    style TenantNotFoundException fill:#ffe1e1
    style UserNotFoundException fill:#ffe1e1
    style UserAlreadyExistsException fill:#ffe1e1
    style DomainInvalidOperationException fill:#ffe1e1
```

---

### The Five Domain Exceptions

#### 1. 🏗️ DomainException (Base Class)

```csharp
namespace Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
```

**Purpose:**
- Base class for all domain exceptions
- Provides consistent exception handling
- Can be caught to handle all domain errors

**Usage Pattern:**
```csharp
try
{
    // Domain operation
}
catch (DomainException ex)
{
    // Handle all domain exceptions
    _logger.LogWarning(ex, "Domain rule violation");
    return BadRequest(ex.Message);
}
```

---

#### 2. 🏢 TenantNotFoundException

```csharp
namespace Domain.Exceptions;

public sealed class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(Guid tenantId)
        : base($"Tenant '{tenantId}' was not found.")
    {
        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}
```

**When to Throw:**
```csharp
public async Task<Tenant> GetTenantAsync(Guid tenantId)
{
    var tenant = await _repository.GetByIdAsync(tenantId);

    if (tenant is null)
        throw new TenantNotFoundException(tenantId);

    return tenant;
}
```

**Exception Flow:**

```mermaid
sequenceDiagram
    participant Client
    participant Handler
    participant Repository

    Client->>Handler: GetTenant(tenantId)
    Handler->>Repository: GetByIdAsync(tenantId)
    Repository-->>Handler: null
    Handler->>Handler: throw TenantNotFoundException
    Handler-->>Client: 404 Not Found

    Note over Handler,Client: Tenant '123' was not found
```

---

#### 3. 👤 UserNotFoundException

```csharp
namespace Domain.Exceptions;

public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid userId)
        : base($"User '{userId}' was not found.")
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}
```

**When to Throw:**
```csharp
public async Task<User> GetUserByIdAsync(Guid userId)
{
    var user = await _repository.GetByIdAsync(userId);

    if (user is null)
        throw new UserNotFoundException(userId);

    return user;
}
```

**Multi-Tenant Context:**

```mermaid
flowchart TD
    Request([Get User Request]) --> ValidateTenant{Tenant Valid?}

    ValidateTenant -->|No| E1[TenantNotFoundException]
    ValidateTenant -->|Yes| GetUser[Query User]

    GetUser --> UserExists{User Found?}

    UserExists -->|No| E2[UserNotFoundException]
    UserExists -->|Yes| CheckTenant{User in Tenant?}

    CheckTenant -->|No| E3[UserNotFoundException<br/>or Unauthorized]
    CheckTenant -->|Yes| Return[Return User ✅]

    style Request fill:#e1f5ff
    style Return fill:#e1ffe1
    style E1 fill:#ffe1e1
    style E2 fill:#ffe1e1
    style E3 fill:#ffe1e1
```

---

#### 4. 🚫 UserAlreadyExistsException

```csharp
namespace Domain.Exceptions;

public sealed class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException(string email)
        : base($"User with email '{email}' already exists.")
    {
        Email = email;
    }

    public string Email { get; }
}
```

**When to Throw:**
```csharp
public async Task<User> CreateUserAsync(CreateUserCommand command)
{
    var existingUser = await _repository.GetByEmailAsync(
        command.TenantId, 
        command.Email);

    if (existingUser is not null)
        throw new UserAlreadyExistsException(command.Email);

    // Create new user...
}
```

**Duplicate Detection Flow:**

```mermaid
sequenceDiagram
    participant Client
    participant Handler
    participant Repository

    Client->>Handler: CreateUser(email)
    Handler->>Repository: GetByEmail(tenantId, email)

    alt User Exists
        Repository-->>Handler: User object
        Handler->>Handler: throw UserAlreadyExistsException
        Handler-->>Client: 409 Conflict
    else User Not Found
        Repository-->>Handler: null
        Handler->>Handler: Create new user
        Handler-->>Client: 201 Created
    end
```

---

#### 5. ⚠️ DomainInvalidOperationException

```csharp
namespace Domain.Exceptions;

public sealed class DomainInvalidOperationException : DomainException
{
    public DomainInvalidOperationException(string message)
        : base(message)
    {
    }

    public DomainInvalidOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

**When to Throw:**
```csharp
public Result Deactivate()
{
    if (!IsActive)
        throw new DomainInvalidOperationException("User is already inactive.");

    IsActive = false;
    return Result.Success();
}

public Result AssignRole(Guid roleId)
{
    if (!IsActive)
        throw new DomainInvalidOperationException(
            "Cannot assign role to inactive user.");

    // Assign role...
}
```

---

### Exception vs Result Pattern

#### When to Use Exceptions ❌

```csharp
// For exceptional, unexpected cases
public User GetUserById(Guid userId)
{
    var user = _repository.GetByIdAsync(userId);

    if (user is null)
        throw new UserNotFoundException(userId);  // ✅ Exceptional case

    return user;
}
```

#### When to Use Result Pattern ✅

```csharp
// For expected business rule violations
public Result<User> Create(string email, string password)
{
    if (string.IsNullOrEmpty(email))
        return Result<User>.Failure(
            Error.Validation("Email is required."));  // ✅ Expected validation

    // Create user...
}
```

**Decision Tree:**

```mermaid
flowchart TD
    Start{Is the error<br/>exceptional?}

    Start -->|Yes| Unexpected{Unexpected<br/>by caller?}
    Start -->|No| Expected[Expected business<br/>rule violation]

    Unexpected -->|Yes| Exception[Use Exception ⚠️]
    Unexpected -->|No| Result1[Use Result Pattern ✅]

    Expected --> Result2[Use Result Pattern ✅]

    Exception --> E1[UserNotFoundException]
    Exception --> E2[TenantNotFoundException]

    Result1 --> R1[Validation errors]
    Result2 --> R2[Business rule violations]

    style Exception fill:#ffe1e1
    style Result1 fill:#e1ffe1
    style Result2 fill:#e1ffe1
```

---

## Repository Interfaces

### What are Repository Interfaces?

**Repository Interfaces** define contracts for data access without exposing implementation details. They:

- Live in the **Domain layer**
- Define **aggregate-focused** operations
- Hide persistence details
- Enable **testing** with mocks
- Support **Clean Architecture** dependency inversion

### Repository Pattern

```mermaid
graph TB
    subgraph "Domain Layer"
        Entity[User Entity]
        IRepo[IUserRepository<br/>Interface]
        Entity -.uses.-> IRepo
    end

    subgraph "Infrastructure Layer"
        Impl[UserRepository<br/>Implementation]
        EF[(EF Core<br/>DbContext)]

        Impl --> EF
    end

    IRepo -.implemented by.-> Impl

    style Entity fill:#e1f5ff
    style IRepo fill:#fff4e1
    style Impl fill:#f0e1ff
    style EF fill:#e1ffe1
```

---

### The Four Repository Interfaces

#### 1. 👤 IUserRepository

```csharp
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
```

**Operations:**

```mermaid
graph LR
    subgraph "Query Operations"
        Q1[GetByIdAsync] --> Single[Single User]
        Q2[GetByEmailAsync] --> Single
        Q3[GetByTenantAsync] --> List[User List]
    end

    subgraph "Command Operations"
        C1[AddAsync] --> New[New User]
        C2[Update] --> Modified[Modified User]
        C3[Remove] --> Deleted[Deleted User]
    end

    style Q1 fill:#e1f5ff
    style Q2 fill:#e1f5ff
    style Q3 fill:#e1f5ff
    style C1 fill:#e1ffe1
    style C2 fill:#fff4e1
    style C3 fill:#ffe1e1
```

**Usage Example:**
```csharp
// Query
var user = await _userRepository.GetByEmailAsync(tenantId, "user@example.com");

// Command
var newUser = User.Create(tenantId, email, hash, "John Doe").Value;
await _userRepository.AddAsync(newUser);
await _unitOfWork.SaveChangesAsync();
```

---

#### 2. 🏢 ITenantRepository

```csharp
using Domain.Entities;

namespace Domain.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    void Update(Tenant tenant);
    void Remove(Tenant tenant);
}
```

**Subdomain Lookup:**

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant Repository
    participant Database

    Client->>API: GET api.acme.example.com
    API->>API: Extract subdomain: "acme"
    API->>Repository: GetBySubdomainAsync("acme")
    Repository->>Database: SELECT * FROM Tenants<br/>WHERE Subdomain = 'acme'
    Database-->>Repository: Tenant record
    Repository-->>API: Tenant object
    API->>API: Set tenant context
    API-->>Client: Response with tenant data
```

---

#### 3. 🎭 IRoleRepository

```csharp
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
```

**Batch Operations:**

```mermaid
flowchart LR
    User[User Entity] --> RoleIds[Role IDs:<br/>123, 456, 789]
    RoleIds --> Repository[IRoleRepository]
    Repository --> Batch[GetByIdsAsync]
    Batch --> DB[(Database)]
    DB --> Roles[Role Objects]
    Roles --> Permissions[Extract<br/>Permissions]

    style User fill:#e1f5ff
    style Repository fill:#fff4e1
    style DB fill:#f0e1ff
    style Permissions fill:#e1ffe1
```

---

#### 4. 💾 IUnitOfWork

```csharp
namespace Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

**Purpose:**
- Coordinates changes across multiple repositories
- Ensures **atomic transactions**
- Dispatches **domain events**
- Manages **database transactions**

**Transaction Pattern:**

```mermaid
sequenceDiagram
    participant Handler
    participant UnitOfWork
    participant UserRepo
    participant RoleRepo
    participant EventBus

    Handler->>UnitOfWork: BeginTransactionAsync()

    Handler->>UserRepo: AddAsync(user)
    Handler->>RoleRepo: AddAsync(role)

    Handler->>UnitOfWork: SaveChangesAsync()

    UnitOfWork->>UnitOfWork: Save to database
    UnitOfWork->>EventBus: Dispatch domain events
    UnitOfWork->>UnitOfWork: CommitTransactionAsync()

    alt Success
        UnitOfWork-->>Handler: Success
    else Failure
        UnitOfWork->>UnitOfWork: RollbackTransactionAsync()
        UnitOfWork-->>Handler: Error
    end
```

**Usage Example:**
```csharp
await _unitOfWork.BeginTransactionAsync();

try
{
    var user = User.Create(...).Value;
    await _userRepository.AddAsync(user);

    var role = Role.Create(...).Value;
    await _roleRepository.AddAsync(role);

    await _unitOfWork.SaveChangesAsync();  // Saves + dispatches events
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## Domain Services

### What are Domain Services?

**Domain Services** encapsulate domain logic that:

- Doesn't naturally fit in an entity
- Operates on **multiple aggregates**
- Requires **external dependencies**
- Represents **domain concepts** (not technical services)

---

### The Two Domain Services

#### 1. 🔒 ITenantIsolationService

```csharp
using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Services;

public interface ITenantIsolationService
{
    bool CanAccess(User actor, TenantId tenantId);
    bool IsIsolated(TenantId tenantId);
    Task<bool> ValidateTenantAccessAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
}
```

**Purpose:**
- Enforce multi-tenancy boundaries
- Validate cross-tenant access
- Implement tenant isolation rules

**Isolation Flow:**

```mermaid
flowchart TD
    Request([API Request]) --> Extract[Extract TenantId<br/>from context]
    Extract --> User[Get User]
    User --> Check{Same Tenant?}

    Check -->|Yes| Allowed[✅ Allow Access]
    Check -->|No| SuperUser{Is SuperUser?}

    SuperUser -->|Yes| Allowed
    SuperUser -->|No| Deny[❌ Deny Access]

    Allowed --> Execute[Execute Operation]
    Deny --> Error[Return 403 Forbidden]

    style Request fill:#e1f5ff
    style Allowed fill:#e1ffe1
    style Deny fill:#ffe1e1
    style Error fill:#ffe1e1
```

**Usage Example:**
```csharp
public async Task<User> GetUserAsync(Guid userId)
{
    var user = await _repository.GetByIdAsync(userId);

    if (!_tenantIsolationService.CanAccess(_currentUser, user.TenantId))
        throw new UnauthorizedAccessException("Cannot access user from different tenant.");

    return user;
}
```

---

#### 2. 🔐 IPasswordHashingService

```csharp
namespace Domain.Services;

public interface IPasswordHashingService
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string passwordHash);
}
```

**Purpose:**
- Abstract password hashing algorithm
- Enable algorithm changes without domain changes
- Support testing with fake implementations

**Authentication Flow:**

```mermaid
sequenceDiagram
    participant User
    participant LoginHandler
    participant HashService
    participant Repository

    User->>LoginHandler: Login(email, password)
    LoginHandler->>Repository: GetByEmailAsync(email)
    Repository-->>LoginHandler: User entity

    LoginHandler->>HashService: Verify(password, user.PasswordHash)

    alt Valid Password
        HashService-->>LoginHandler: true
        LoginHandler->>LoginHandler: Generate JWT token
        LoginHandler-->>User: Success + Token
    else Invalid Password
        HashService-->>LoginHandler: false
        LoginHandler-->>User: 401 Unauthorized
    end
```

**Usage Example:**
```csharp
// Registration
var passwordHash = _passwordHashingService.Hash(command.Password);
var user = User.Create(tenantId, email, passwordHash, fullName).Value;

// Login
var user = await _repository.GetByEmailAsync(tenantId, command.Email);
var isValid = _passwordHashingService.Verify(command.Password, user.PasswordHash);

if (!isValid)
    return Result.Failure(Error.Unauthorized("Invalid credentials."));
```

---

## Integration Patterns

### Complete Request Flow

```mermaid
sequenceDiagram
    participant API
    participant Handler
    participant Service
    participant Repository
    participant UnitOfWork
    participant EventBus

    API->>Handler: CreateUserCommand
    Handler->>Service: Hash password
    Service-->>Handler: passwordHash

    Handler->>Handler: User.Create()
    Handler->>Repository: AddAsync(user)

    Handler->>UnitOfWork: SaveChangesAsync()
    UnitOfWork->>UnitOfWork: Persist to database
    UnitOfWork->>EventBus: Dispatch UserCreatedEvent

    EventBus->>EventBus: Trigger handlers

    UnitOfWork-->>Handler: Success
    Handler-->>API: Result<UserDto>
```

---

## Best Practices

### ✅ DO

1. **Define Interfaces in Domain**
   ```csharp
   // Domain/Repositories/IUserRepository.cs
   namespace Domain.Repositories;

   public interface IUserRepository { ... }
   ```

2. **Implement in Infrastructure**
   ```csharp
   // Infrastructure/Persistence/UserRepository.cs
   namespace Infrastructure.Persistence;

   public class UserRepository : IUserRepository { ... }
   ```

3. **Use Aggregate-Focused Methods**
   ```csharp
   Task<User?> GetByIdAsync(Guid userId);  // ✅ Returns aggregate root
   Task<string> GetEmailByIdAsync(Guid userId);  // ❌ Returns partial data
   ```

4. **Return Domain Types**
   ```csharp
   Task<User> GetByIdAsync(Guid userId);  // ✅ Domain entity
   Task<UserDto> GetByIdAsync(Guid userId);  // ❌ DTO in domain
   ```

### ❌ DON'T

1. **Don't Put Logic in Repositories**
   ```csharp
   // ❌ Business logic in repository
   public interface IUserRepository
   {
       Task<bool> ValidateUserCanLogin(Guid userId);
   }

   // ✅ Business logic in entity/service
   user.CanLogin();
   ```

2. **Don't Expose IQueryable**
   ```csharp
   // ❌ Exposes implementation details
   IQueryable<User> GetUsers();

   // ✅ Well-defined contract
   Task<IReadOnlyList<User>> GetByTenantAsync(TenantId tenantId);
   ```

3. **Don't Reference Infrastructure**
   ```csharp
   // ❌ Domain depends on infrastructure
   using Microsoft.EntityFrameworkCore;

   public interface IUserRepository
   {
       DbSet<User> Users { get; }
   }
   ```

---

## Summary

### Domain Exceptions
- ✅ Meaningful business errors
- ✅ Hierarchical structure
- ✅ Include relevant context
- ✅ Used for exceptional cases

### Repository Interfaces
- ✅ Aggregate-focused operations
- ✅ Hide persistence details
- ✅ Enable testing
- ✅ Support dependency inversion

### Domain Services
- ✅ Cross-aggregate logic
- ✅ Domain concepts
- ✅ Abstract technical concerns
- ✅ Testable contracts

```mermaid
mindmap
  root((Phase 2 Day 4))
    Exceptions
      DomainException base
      Specific errors
      Business rule violations
    Repositories
      IUserRepository
      ITenantRepository
      IRoleRepository
      IUnitOfWork
    Services
      Tenant isolation
      Password hashing
```

---

**Phase 2 Complete!** Next: [Phase 3 - Application Layer](../IMPLEMENTATION_PLAN.md#phase-3)

**Last Updated:** April 02, 2026
