# Domain Entities - Deep Dive

## 📖 Table of Contents
- [What are Domain Entities?](#what-are-domain-entities)
- [Entity Design Principles](#entity-design-principles)
- [The Four Core Entities](#the-four-core-entities)
- [Entity Relationships](#entity-relationships)
- [Best Practices](#best-practices)

---

## What are Domain Entities?

**Domain Entities** are objects that have a **unique identity** and **lifecycle**. Unlike value objects, entities are:

- Identified by their ID (not by their attributes)
- Mutable (can change state over time)
- Responsible for maintaining invariants
- Capable of raising domain events

### Entity vs Value Object

```mermaid
graph LR
    subgraph "Entity"
        E1[User ID: 123<br/>Email: john@example.com]
        E2[User ID: 123<br/>Email: jane@example.com]
        E1 -.same identity.-> E2
    end

    subgraph "Value Object"
        V1[Email: john@example.com]
        V2[Email: john@example.com]
        V3[Email: jane@example.com]
        V1 -.equal.-> V2
        V1 -.not equal.-> V3
    end

    style E1 fill:#ffe1e1
    style E2 fill:#ffe1e1
    style V1 fill:#e1ffe1
    style V2 fill:#e1ffe1
    style V3 fill:#e1ffe1
```

**Key Difference:** Two entities with the same ID are the same entity, even if their properties differ. Two value objects are equal only if all their properties match.

---

## Entity Design Principles

### 1. Encapsulation
```csharp
// ❌ BAD: Public setters expose internals
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}

// ✅ GOOD: Private setters, public methods
public class User : AggregateRoot
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    public bool IsActive { get; private set; }

    public Result Deactivate()
    {
        if (!IsActive) return Result.Success();
        IsActive = false;
        AddDomainEvent(new UserDeactivatedEvent(Id));
        return Result.Success();
    }
}
```

### 2. Aggregate Roots
Only **aggregate roots** can be accessed from outside. Internal entities must go through the root.

```mermaid
graph TB
    subgraph "Aggregate Boundary"
        AR[🎯 User<br/>Aggregate Root]
        E1[Role Assignment<br/>Internal]
        E2[Permissions<br/>Internal]

        AR -->|manages| E1
        E1 -->|references| E2
    end

    External[External Code] -->|can only access| AR
    External -.cannot access.-> E1
    External -.cannot access.-> E2

    style AR fill:#ffd700
    style E1 fill:#e1f5ff
    style E2 fill:#e1f5ff
    style External fill:#ffe1e1
```

### 3. Factory Methods
Use static factory methods to enforce creation rules:

```csharp
public static Result<User> Create(
    TenantId tenantId, 
    Email email, 
    string passwordHash, 
    string fullName)
{
    // Validation
    if (string.IsNullOrWhiteSpace(passwordHash))
        return Result<User>.Failure(Error.Validation("Password hash is required."));

    if (string.IsNullOrWhiteSpace(fullName))
        return Result<User>.Failure(Error.Validation("Full name is required."));

    // Create entity
    var user = new User(tenantId, email, passwordHash, fullName.Trim());

    // Raise domain event
    user.AddDomainEvent(new UserCreatedEvent(user.Id, user.TenantId.Value, user.Email.Value));

    return Result<User>.Success(user);
}
```

### 4. Domain Events
Entities raise events for significant state changes:

```mermaid
sequenceDiagram
    participant Client
    participant User
    participant EventBus
    participant Handler

    Client->>User: Deactivate()
    User->>User: Validate state
    User->>User: Change IsActive = false
    User->>User: AddDomainEvent(UserDeactivatedEvent)
    User-->>Client: Result.Success()

    Client->>EventBus: SaveChanges()
    EventBus->>Handler: Publish(UserDeactivatedEvent)
    Handler->>Handler: Send notification email
    Handler->>Handler: Update analytics
```

---

## The Four Core Entities

### 1. 👤 User Entity

**Purpose:** Manages user authentication, authorization, and lifecycle within a tenant.

```mermaid
classDiagram
    class User {
        +Guid Id
        +TenantId TenantId
        +Email Email
        +string PasswordHash
        +string FullName
        +bool IsActive
        +IReadOnlyCollection~Guid~ RoleIds

        +Create(tenantId, email, hash, name)$ Result~User~
        +AssignRole(roleId) Result
        +Deactivate() Result
        +UpdatePassword(newHash) Result
    }

    class AggregateRoot {
        <<abstract>>
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        #AddDomainEvent(event)
        +ClearDomainEvents()
    }

    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAtUtc
        +DateTime? UpdatedAtUtc
        #MarkUpdated()
    }

    User --|> AggregateRoot
    AggregateRoot --|> BaseEntity
```

**Key Responsibilities:**
- ✅ User authentication (password management)
- ✅ Role assignment within tenant
- ✅ Account activation/deactivation
- ✅ User profile management

**Business Rules:**
```csharp
// Cannot assign role to inactive user
public Result AssignRole(Guid roleId)
{
    if (!IsActive)
        return Result.Failure(Error.Conflict("Cannot assign role to inactive user."));

    if (roleId == Guid.Empty)
        return Result.Failure(Error.Validation("RoleId cannot be empty."));

    if (_roleIds.Add(roleId))
    {
        MarkUpdated();
        AddDomainEvent(new RoleAssignedEvent(Id, roleId, TenantId.Value));
    }

    return Result.Success();
}
```

**Domain Events:**
- `UserCreatedEvent` - New user registered
- `UserDeactivatedEvent` - User account disabled
- `RoleAssignedEvent` - Role granted to user
- `PasswordChangedEvent` - Password updated

---

### 2. 🏢 Tenant Entity

**Purpose:** Represents a customer organization with isolated data and configuration.

```mermaid
classDiagram
    class Tenant {
        +Guid Id
        +string Name
        +string Subdomain
        +SubscriptionTier Tier
        +bool IsActive

        +Create(name, subdomain, tier)$ Result~Tenant~
        +Activate() void
        +Deactivate() void
        +UpdateSettings(name, tier) Result
    }

    class SubscriptionTier {
        <<enumeration>>
        Free
        Basic
        Premium
        Enterprise
    }

    Tenant --> SubscriptionTier
```

**Key Responsibilities:**
- ✅ Tenant provisioning and configuration
- ✅ Subscription tier management
- ✅ Tenant activation/deactivation
- ✅ Subdomain uniqueness

**Business Rules:**
```csharp
public static Result<Tenant> Create(string name, string subdomain, SubscriptionTier tier)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result<Tenant>.Failure(Error.Validation("Tenant name is required."));

    // Subdomain must be valid URL-safe string
    if (string.IsNullOrWhiteSpace(subdomain) || subdomain.Contains(' '))
        return Result<Tenant>.Failure(Error.Validation("Subdomain is invalid."));

    var tenant = new Tenant(
        name.Trim(), 
        subdomain.Trim().ToLowerInvariant(), 
        tier);

    tenant.AddDomainEvent(new TenantProvisionedEvent(tenant.Id, tenant.Name, tenant.Subdomain));

    return Result<Tenant>.Success(tenant);
}
```

**Domain Events:**
- `TenantProvisionedEvent` - New tenant created

**Multi-Tenancy Pattern:**
```mermaid
graph TB
    subgraph "Tenant A"
        TA[Tenant: Acme Corp<br/>Subdomain: acme]
        UA1[User: Alice]
        UA2[User: Bob]
        RA[Role: Admin]

        TA --> UA1
        TA --> UA2
        TA --> RA
    end

    subgraph "Tenant B"
        TB[Tenant: Beta Inc<br/>Subdomain: beta]
        UB1[User: Charlie]
        UB2[User: Diana]
        RB[Role: Manager]

        TB --> UB1
        TB --> UB2
        TB --> RB
    end

    Database[(Shared Database)]

    TA --> Database
    TB --> Database

    style TA fill:#e1f5ff
    style TB fill:#f0e1ff
    style Database fill:#ffe1e1
```

---

### 3. 🎭 Role Entity

**Purpose:** Manages permissions within a tenant scope.

```mermaid
classDiagram
    class Role {
        +Guid Id
        +TenantId TenantId
        +string Name
        +bool IsSystemRole
        +IReadOnlyCollection~string~ Permissions

        +Create(tenantId, name, isSystemRole)$ Result~Role~
        +AssignPermission(permission) Result
        +RevokePermission(permission) Result
    }

    class Permission {
        +string Name
        +string Resource
        +string Action

        +Of(resource, action)$ Permission
        +UsersRead()$ Permission
        +UsersWrite()$ Permission
        +TenantsManage()$ Permission
    }

    Role --> Permission : contains many
```

**Key Responsibilities:**
- ✅ Permission aggregation
- ✅ Role naming and identification
- ✅ System vs custom role distinction
- ✅ Tenant-scoped role isolation

**Business Rules:**
```csharp
// System roles cannot have permissions revoked
public Result RevokePermission(string permission)
{
    if (IsSystemRole)
        return Result.Failure(
            Error.Conflict("Cannot revoke permissions from a system role."));

    if (_permissions.Remove(permission.Trim()))
        MarkUpdated();

    return Result.Success();
}
```

**Permission Model:**
```mermaid
graph LR
    subgraph "Permission Structure"
        P1[users:read]
        P2[users:write]
        P3[tenants:manage]
        P4[roles:assign]
    end

    subgraph "Roles"
        R1[Admin Role] --> P1
        R1 --> P2
        R1 --> P3
        R1 --> P4

        R2[User Role] --> P1
    end

    subgraph "Users"
        U1[Alice] -.has.-> R1
        U2[Bob] -.has.-> R2
    end

    style P1 fill:#e1ffe1
    style P2 fill:#e1ffe1
    style P3 fill:#ffe1e1
    style P4 fill:#ffe1e1
    style R1 fill:#fff4e1
    style R2 fill:#fff4e1
```

---

### 4. 🔐 Permission Entity

**Purpose:** Defines canonical permissions using resource:action pattern.

```csharp
public sealed class Permission : BaseEntity
{
    public string Name { get; private set; }      // "users:read"
    public string Resource { get; private set; }  // "users"
    public string Action { get; private set; }    // "read"

    public static Permission Of(string resource, string action)
    {
        var normalizedResource = resource.Trim().ToLowerInvariant();
        var normalizedAction = action.Trim().ToLowerInvariant();
        return new Permission(
            $"{normalizedResource}:{normalizedAction}", 
            normalizedResource, 
            normalizedAction);
    }

    // Predefined permissions
    public static Permission UsersRead() => Of("users", "read");
    public static Permission UsersWrite() => Of("users", "write");
    public static Permission TenantsManage() => Of("tenants", "manage");
}
```

**Permission Hierarchy:**
```mermaid
graph TB
    subgraph "Resources"
        R1[Users]
        R2[Tenants]
        R3[Roles]
        R4[Reports]
    end

    subgraph "Actions"
        A1[Read]
        A2[Write]
        A3[Delete]
        A4[Manage]
    end

    subgraph "Permissions"
        P1[users:read]
        P2[users:write]
        P3[users:delete]
        P4[tenants:manage]
        P5[roles:write]
    end

    R1 --> P1
    R1 --> P2
    R1 --> P3
    R2 --> P4
    R3 --> P5

    style R1 fill:#e1f5ff
    style R2 fill:#e1f5ff
    style R3 fill:#e1f5ff
    style R4 fill:#e1f5ff
    style A1 fill:#e1ffe1
    style A2 fill:#e1ffe1
    style A3 fill:#ffe1e1
    style A4 fill:#ffe1e1
```

---

## Entity Relationships

### Complete Domain Model

```mermaid
erDiagram
    TENANT ||--o{ USER : "has many"
    TENANT ||--o{ ROLE : "defines"
    USER ||--o{ ROLE : "assigned to"
    ROLE ||--o{ PERMISSION : "contains"

    TENANT {
        Guid Id PK
        string Name
        string Subdomain UK
        SubscriptionTier Tier
        bool IsActive
    }

    USER {
        Guid Id PK
        Guid TenantId FK
        Email Email UK
        string PasswordHash
        string FullName
        bool IsActive
    }

    ROLE {
        Guid Id PK
        Guid TenantId FK
        string Name
        bool IsSystemRole
    }

    PERMISSION {
        Guid Id PK
        string Name UK
        string Resource
        string Action
    }
```

### Aggregate Boundaries

```mermaid
graph TB
    subgraph "User Aggregate"
        U[👤 User<br/>Aggregate Root]
        UR[Role IDs<br/>Collection]
        U --> UR
    end

    subgraph "Tenant Aggregate"
        T[🏢 Tenant<br/>Aggregate Root]
        TS[Settings<br/>Value Objects]
        T --> TS
    end

    subgraph "Role Aggregate"
        R[🎭 Role<br/>Aggregate Root]
        RP[Permissions<br/>Collection]
        R --> RP
    end

    U -.references.-> T
    U -.references.-> R
    R -.references.-> T

    style U fill:#ffd700
    style T fill:#ffd700
    style R fill:#ffd700
```

---

## Best Practices

### ✅ DO

1. **Use Factory Methods**
   ```csharp
   var userResult = User.Create(tenantId, email, hash, "John Doe");
   if (userResult.IsSuccess)
       await _repository.AddAsync(userResult.Value);
   ```

2. **Encapsulate State Changes**
   ```csharp
   public Result Deactivate()
   {
       IsActive = false;
       MarkUpdated();
       AddDomainEvent(new UserDeactivatedEvent(Id, TenantId.Value));
       return Result.Success();
   }
   ```

3. **Validate Invariants**
   ```csharp
   if (!IsActive)
       return Result.Failure(Error.Conflict("User is not active."));
   ```

4. **Raise Domain Events**
   ```csharp
   AddDomainEvent(new UserCreatedEvent(Id, TenantId.Value, Email.Value));
   ```

### ❌ DON'T

1. **Don't Use Public Setters**
   ```csharp
   // ❌ Exposes internal state
   public string Email { get; set; }
   ```

2. **Don't Create Anemic Models**
   ```csharp
   // ❌ Just a data container
   public class User
   {
       public Guid Id { get; set; }
       public string Email { get; set; }
   }
   ```

3. **Don't Access Other Aggregates Directly**
   ```csharp
   // ❌ Crossing aggregate boundaries
   var role = user.Tenant.Roles.First();
   ```

4. **Don't Perform Infrastructure Operations**
   ```csharp
   // ❌ Database access in entity
   public void Save() => _dbContext.SaveChanges();
   ```

---

## Summary

| Aspect | Description |
|--------|-------------|
| **Identity** | Entities have unique IDs |
| **Lifecycle** | Created, modified, deleted |
| **Encapsulation** | Private setters, public methods |
| **Validation** | Factory methods + business rules |
| **Events** | Raise events for state changes |
| **Boundaries** | Respect aggregate boundaries |
| **Purity** | No infrastructure dependencies |

---

**Next:** Learn about [Value Objects](./ValueObjects.md) for type safety and immutability.

**Last Updated:** April 02, 2026
