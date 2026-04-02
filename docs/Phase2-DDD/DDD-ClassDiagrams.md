# DDD Core Concepts - Visual Class Diagram

> **Complete visual representation of Domain-Driven Design patterns and relationships in this project**

This document provides comprehensive class diagrams showing how all DDD concepts relate to each other in the domain layer.

> **📝 Note:** All diagrams are optimized for both light and dark themes. Colors are handled by your Mermaid renderer for better compatibility.

---

## 📊 Table of Contents

- [Complete Domain Layer Overview](#complete-domain-layer-overview)
- [Foundational Building Blocks](#foundational-building-blocks)
- [Entity Hierarchy](#entity-hierarchy)
- [Value Object System](#value-object-system)
- [Domain Events Architecture](#domain-events-architecture)
- [Repository & Service Contracts](#repository--service-contracts)
- [Exception Hierarchy](#exception-hierarchy)
- [Complete Integration Diagram](#complete-integration-diagram)

---

## Complete Domain Layer Overview

### High-Level Architecture

```mermaid
classDiagram
    %% Foundational Layer
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAtUtc
        +DateTime? UpdatedAtUtc
        +bool IsDeleted
        #MarkUpdated()
        #SoftDelete()
    }

    class AggregateRoot {
        <<abstract>>
        -List~IDomainEvent~ _domainEvents
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        #AddDomainEvent(IDomainEvent)
        +ClearDomainEvents()
    }

    class ValueObject {
        <<abstract>>
        #GetEqualityComponents()* IEnumerable~object~
        +Equals(ValueObject) bool
        +GetHashCode() int
        +operator ==(ValueObject, ValueObject) bool
        +operator !=(ValueObject, ValueObject) bool
    }

    class Result {
        +bool IsSuccess
        +bool IsFailure
        +Error Error
        +Success()$ Result
        +Failure(Error)$ Result
    }

    class Error {
        +string Code
        +string Message
        +ErrorType Type
        +None$ Error
        +Validation(string)$ Error
        +NotFound(string)$ Error
        +Conflict(string)$ Error
        +Unauthorized(string)$ Error
    }

    %% Relationships
    AggregateRoot --|> BaseEntity : inherits
    Result --> Error : contains

    %% Note: Styles removed for better theme compatibility
    %% Works in both light and dark modes
```

---

## Foundational Building Blocks

### Core Abstractions Detail

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAtUtc
        +DateTime? UpdatedAtUtc
        +bool IsDeleted
        #BaseEntity()
        #BaseEntity(Guid?)
        #MarkUpdated() void
        #SoftDelete() void
        #Restore() void
    }

    class AggregateRoot {
        <<abstract>>
        -List~IDomainEvent~ _domainEvents
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        #AggregateRoot()
        #AggregateRoot(Guid?)
        #AddDomainEvent(IDomainEvent) void
        +ClearDomainEvents() void
    }

    class IDomainEvent {
        <<interface>>
        +DateTime OccurredOnUtc
    }

    class ValueObject {
        <<abstract>>
        #GetEqualityComponents()* IEnumerable~object~
        +Equals(ValueObject?) bool
        +Equals(object?) bool
        +GetHashCode() int
        +operator ==(ValueObject?, ValueObject?) bool
        +operator !=(ValueObject?, ValueObject?) bool
    }

    class Result {
        -bool _isSuccess
        -Error _error
        +bool IsSuccess
        +bool IsFailure
        +Error Error
        -Result(bool, Error)
        +Success()$ Result
        +Failure(Error)$ Result
    }

    class Result~T~ {
        -T? _value
        +T Value
        +Success(T)$ Result~T~
        +Failure(Error)$ Result~T~
    }

    class Error {
        +string Code
        +string Message
        +ErrorType Type
        +None$ Error
        +Validation(string)$ Error
        +Validation(string, string)$ Error
        +NotFound(string)$ Error
        +NotFound(string, object)$ Error
        +Conflict(string)$ Error
        +Unauthorized(string)$ Error
        +Forbidden(string)$ Error
    }

    class ErrorType {
        <<enumeration>>
        Validation
        NotFound
        Conflict
        Unauthorized
        Forbidden
        Internal
    }

    AggregateRoot --|> BaseEntity
    AggregateRoot o-- IDomainEvent : collects
    Result~T~ --|> Result
    Result --> Error
    Error --> ErrorType

    note for BaseEntity "Base class for all entities\nProvides identity & audit"
    note for AggregateRoot "Collects domain events\nDefines boundaries"
    note for ValueObject "Equality by value\nImmutable objects"
    note for Result "Railway-oriented\nprogramming"
```

---

## Entity Hierarchy

### Complete Entity Structure

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAtUtc
        +DateTime? UpdatedAtUtc
        +bool IsDeleted
    }

    class AggregateRoot {
        <<abstract>>
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        #AddDomainEvent(IDomainEvent)
        +ClearDomainEvents()
    }

    class User {
        -HashSet~Guid~ _roleIds
        +TenantId TenantId
        +Email Email
        +string PasswordHash
        +string FullName
        +bool IsActive
        +IReadOnlyCollection~Guid~ RoleIds
        +Create(TenantId, Email, string, string)$ Result~User~
        +AssignRole(Guid) Result
        +RemoveRole(Guid) Result
        +Deactivate() Result
        +Activate() Result
        +UpdatePassword(string) Result
        +UpdateProfile(string) Result
    }

    class Tenant {
        +string Name
        +string Subdomain
        +SubscriptionTier Tier
        +bool IsActive
        +Create(string, string, SubscriptionTier)$ Result~Tenant~
        +Activate() void
        +Deactivate() void
        +UpdateSettings(string, SubscriptionTier) Result
        +UpgradeTier(SubscriptionTier) Result
        +DowngradeTier(SubscriptionTier) Result
    }

    class Role {
        -HashSet~string~ _permissions
        +TenantId TenantId
        +string Name
        +bool IsSystemRole
        +IReadOnlyCollection~string~ Permissions
        +Create(TenantId, string, bool)$ Result~Role~
        +AssignPermission(string) Result
        +RevokePermission(string) Result
        +UpdateName(string) Result
        +HasPermission(string) bool
    }

    class Permission {
        +string Name
        +string Resource
        +string Action
        +Of(string, string)$ Permission
        +UsersRead()$ Permission
        +UsersWrite()$ Permission
        +UsersDelete()$ Permission
        +TenantsManage()$ Permission
        +RolesRead()$ Permission
        +RolesWrite()$ Permission
    }

    BaseEntity <|-- AggregateRoot
    AggregateRoot <|-- User
    AggregateRoot <|-- Tenant
    AggregateRoot <|-- Role
    BaseEntity <|-- Permission

    User --> TenantId : has
    User --> Email : has
    User --> Role : references by ID
    Tenant --> SubscriptionTier : has
    Role --> TenantId : scoped by
    Role --> Permission : contains many

    note for User "Aggregate Root\nManages authentication\nand authorization"
    note for Tenant "Aggregate Root\nMulti-tenancy isolation"
    note for Role "Aggregate Root\nPermission aggregation"
    note for Permission "Entity (not aggregate)\nDefines access rights"
```

### Entity Relationships (ERD Style)

```mermaid
erDiagram
    TENANT ||--o{ USER : "has many"
    TENANT ||--o{ ROLE : "defines"
    USER }o--o{ ROLE : "assigned to"
    ROLE ||--o{ PERMISSION : "contains"

    TENANT {
        Guid Id PK
        string Name
        string Subdomain UK
        int Tier
        bool IsActive
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc
    }

    USER {
        Guid Id PK
        Guid TenantId FK
        string Email UK
        string PasswordHash
        string FullName
        bool IsActive
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc
    }

    ROLE {
        Guid Id PK
        Guid TenantId FK
        string Name
        bool IsSystemRole
        DateTime CreatedAtUtc
        DateTime UpdatedAtUtc
    }

    PERMISSION {
        Guid Id PK
        string Name UK
        string Resource
        string Action
    }

    USER_ROLES {
        Guid UserId FK
        Guid RoleId FK
    }

    ROLE_PERMISSIONS {
        Guid RoleId FK
        Guid PermissionId FK
    }
```

---

## Value Object System

### Value Objects Detail

```mermaid
classDiagram
    class ValueObject {
        <<abstract>>
        #GetEqualityComponents()* IEnumerable~object~
        +Equals(ValueObject?) bool
        +GetHashCode() int
        +operator ==(ValueObject?, ValueObject?) bool
        +operator !=(ValueObject?, ValueObject?) bool
    }

    class Email {
        +string Value
        +MaxLength$ int
        -Email(string)
        +Create(string)$ Result~Email~
        -IsValidFormat(string)$ bool
        #GetEqualityComponents() IEnumerable~object~
    }

    class Password {
        +string Value
        +MinLength$ int
        +MaxLength$ int
        -Password(string)
        +Create(string)$ Result~Password~
        -HasRequiredComplexity(string)$ bool
        -HasUpperCase(string)$ bool
        -HasLowerCase(string)$ bool
        -HasDigit(string)$ bool
        -HasSpecialChar(string)$ bool
        #GetEqualityComponents() IEnumerable~object~
    }

    class TenantId {
        +Guid Value
        -TenantId(Guid)
        +Create(Guid)$ Result~TenantId~
        +CreateUnsafe(Guid)$ TenantId
        +NewId()$ TenantId
        #GetEqualityComponents() IEnumerable~object~
    }

    class SubscriptionTier {
        +int Value
        +string Name
        +int MaxUsers
        +int MaxRoles
        +Free$ SubscriptionTier
        +Basic$ SubscriptionTier
        +Premium$ SubscriptionTier
        +Enterprise$ SubscriptionTier
        -SubscriptionTier(int, string, int, int)
        +FromValue(int)$ SubscriptionTier
        +FromName(string)$ SubscriptionTier
        #GetEqualityComponents() IEnumerable~object~
    }

    ValueObject <|-- Email
    ValueObject <|-- Password
    ValueObject <|-- TenantId
    ValueObject <|-- SubscriptionTier

    note for Email "Validates email format\nNormalizes to lowercase"
    note for Password "Enforces complexity\nMin 8 chars required"
    note for TenantId "Strongly-typed\ntenant identifier"
    note for SubscriptionTier "Enum pattern\nwith business logic"
```

---

## Domain Events Architecture

### Domain Events Hierarchy

```mermaid
classDiagram
    class IDomainEvent {
        <<interface>>
        +DateTime OccurredOnUtc
    }

    class UserCreatedEvent {
        <<record>>
        +Guid UserId
        +Guid TenantId
        +string Email
        +DateTime OccurredOnUtc
        +UserCreatedEvent(Guid, Guid, string)
    }

    class UserDeactivatedEvent {
        <<record>>
        +Guid UserId
        +Guid TenantId
        +DateTime OccurredOnUtc
        +UserDeactivatedEvent(Guid, Guid)
    }

    class RoleAssignedEvent {
        <<record>>
        +Guid UserId
        +Guid RoleId
        +Guid TenantId
        +DateTime OccurredOnUtc
        +RoleAssignedEvent(Guid, Guid, Guid)
    }

    class PasswordChangedEvent {
        <<record>>
        +Guid UserId
        +Guid TenantId
        +DateTime OccurredOnUtc
        +PasswordChangedEvent(Guid, Guid)
    }

    class TenantProvisionedEvent {
        <<record>>
        +Guid TenantId
        +string Name
        +string Subdomain
        +DateTime OccurredOnUtc
        +TenantProvisionedEvent(Guid, string, string)
    }

    class RoleRevokedEvent {
        <<record>>
        +Guid UserId
        +Guid RoleId
        +Guid TenantId
        +DateTime OccurredOnUtc
        +RoleRevokedEvent(Guid, Guid, Guid)
    }

    IDomainEvent <|.. UserCreatedEvent : implements
    IDomainEvent <|.. UserDeactivatedEvent : implements
    IDomainEvent <|.. RoleAssignedEvent : implements
    IDomainEvent <|.. PasswordChangedEvent : implements
    IDomainEvent <|.. TenantProvisionedEvent : implements
    IDomainEvent <|.. RoleRevokedEvent : implements

    note for IDomainEvent "Marker interface\nfor all domain events"
    note for UserCreatedEvent "Raised when\nuser is created"
    note for TenantProvisionedEvent "Raised when\ntenant is created"
```

---

## Repository & Service Contracts

### Repository Interfaces

```mermaid
classDiagram
    class IUserRepository {
        <<interface>>
        +GetByIdAsync(Guid, CancellationToken) Task~User?~
        +GetByEmailAsync(TenantId, string, CancellationToken) Task~User?~
        +GetByTenantAsync(TenantId, CancellationToken) Task~IReadOnlyList~User~~
        +AddAsync(User, CancellationToken) Task
        +Update(User) void
        +Remove(User) void
    }

    class ITenantRepository {
        <<interface>>
        +GetByIdAsync(Guid, CancellationToken) Task~Tenant?~
        +GetBySubdomainAsync(string, CancellationToken) Task~Tenant?~
        +GetAllAsync(CancellationToken) Task~IReadOnlyList~Tenant~~
        +AddAsync(Tenant, CancellationToken) Task
        +Update(Tenant) void
        +Remove(Tenant) void
    }

    class IRoleRepository {
        <<interface>>
        +GetByIdAsync(Guid, CancellationToken) Task~Role?~
        +GetByNameAsync(TenantId, string, CancellationToken) Task~Role?~
        +GetByTenantAsync(TenantId, CancellationToken) Task~IReadOnlyList~Role~~
        +GetByIdsAsync(IEnumerable~Guid~, CancellationToken) Task~IReadOnlyList~Role~~
        +AddAsync(Role, CancellationToken) Task
        +Update(Role) void
        +Remove(Role) void
    }

    class IUnitOfWork {
        <<interface>>
        +SaveChangesAsync(CancellationToken) Task~int~
        +BeginTransactionAsync(CancellationToken) Task
        +CommitTransactionAsync(CancellationToken) Task
        +RollbackTransactionAsync(CancellationToken) Task
    }

    IUserRepository --> User : queries
    ITenantRepository --> Tenant : queries
    IRoleRepository --> Role : queries

    note for IUserRepository "Aggregate-focused\ndata access"
    note for IUnitOfWork "Transaction\ncoordination"
```

### Domain Services

```mermaid
classDiagram
    class ITenantIsolationService {
        <<interface>>
        +CanAccess(User, TenantId) bool
        +IsIsolated(TenantId) bool
        +ValidateTenantAccessAsync(Guid, Guid, CancellationToken) Task~bool~
    }

    class IPasswordHashingService {
        <<interface>>
        +Hash(string) string
        +Verify(string, string) bool
    }

    ITenantIsolationService --> User : validates
    ITenantIsolationService --> TenantId : checks
    IPasswordHashingService ..> Password : works with

    note for ITenantIsolationService "Multi-tenancy\nenforcement"
    note for IPasswordHashingService "Password security\nabstraction"
```

---

## Exception Hierarchy

### Domain Exceptions

```mermaid
classDiagram
    class Exception {
        <<.NET Framework>>
        +string Message
        +Exception InnerException
    }

    class DomainException {
        <<abstract>>
        #DomainException(string)
        #DomainException(string, Exception)
    }

    class TenantNotFoundException {
        +Guid TenantId
        +TenantNotFoundException(Guid)
    }

    class UserNotFoundException {
        +Guid UserId
        +UserNotFoundException(Guid)
    }

    class UserAlreadyExistsException {
        +string Email
        +UserAlreadyExistsException(string)
    }

    class DomainInvalidOperationException {
        +DomainInvalidOperationException(string)
        +DomainInvalidOperationException(string, Exception)
    }

    Exception <|-- DomainException
    DomainException <|-- TenantNotFoundException
    DomainException <|-- UserNotFoundException
    DomainException <|-- UserAlreadyExistsException
    DomainException <|-- DomainInvalidOperationException

    note for DomainException "Base for all\ndomain exceptions"
    note for TenantNotFoundException "Tenant not found\nin query"
    note for UserAlreadyExistsException "Duplicate email\ndetected"
```

---

## Complete Integration Diagram

### Full Domain Layer Integration

```mermaid
classDiagram
    %% Foundation
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAtUtc
    }

    class AggregateRoot {
        <<abstract>>
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
    }

    class ValueObject {
        <<abstract>>
        #GetEqualityComponents()*
    }

    %% Entities
    class User {
        +TenantId TenantId
        +Email Email
        +Create()$
        +AssignRole()
    }

    class Tenant {
        +string Name
        +SubscriptionTier Tier
        +Create()$
    }

    class Role {
        +TenantId TenantId
        +string Name
        +Create()$
    }

    %% Value Objects
    class Email {
        +string Value
        +Create()$
    }

    class TenantId {
        +Guid Value
        +Create()$
    }

    class SubscriptionTier {
        +string Name
        +int MaxUsers
    }

    %% Events
    class IDomainEvent {
        <<interface>>
        +DateTime OccurredOnUtc
    }

    class UserCreatedEvent {
        +Guid UserId
    }

    class TenantProvisionedEvent {
        +Guid TenantId
    }

    %% Repositories
    class IUserRepository {
        <<interface>>
        +GetByIdAsync()
    }

    class ITenantRepository {
        <<interface>>
        +GetByIdAsync()
    }

    class IUnitOfWork {
        <<interface>>
        +SaveChangesAsync()
    }

    %% Services
    class ITenantIsolationService {
        <<interface>>
        +CanAccess()
    }

    %% Exceptions
    class DomainException {
        <<abstract>>
    }

    class UserNotFoundException {
    }

    %% Result Pattern
    class Result {
        +bool IsSuccess
        +Error Error
    }

    class Error {
        +string Code
        +string Message
    }

    %% Relationships
    BaseEntity <|-- AggregateRoot
    AggregateRoot <|-- User
    AggregateRoot <|-- Tenant
    AggregateRoot <|-- Role

    ValueObject <|-- Email
    ValueObject <|-- TenantId
    ValueObject <|-- SubscriptionTier

    User --> Email
    User --> TenantId
    Tenant --> SubscriptionTier
    Role --> TenantId

    AggregateRoot o-- IDomainEvent
    IDomainEvent <|.. UserCreatedEvent
    IDomainEvent <|.. TenantProvisionedEvent

    IUserRepository --> User
    ITenantRepository --> Tenant

    ITenantIsolationService --> User
    ITenantIsolationService --> TenantId

    DomainException <|-- UserNotFoundException

    Result --> Error

    User ..> Result : returns
    Tenant ..> Result : returns
    Role ..> Result : returns
    Email ..> Result : returns
```

---

## Usage Examples

### How Entities Use Value Objects

```mermaid
sequenceDiagram
    participant Client
    participant User
    participant Email
    participant TenantId
    participant Result

    Client->>Email: Create("user@example.com")
    Email->>Email: Validate format
    Email-->>Client: Result<Email>.Success(email)

    Client->>TenantId: Create(guid)
    TenantId->>TenantId: Validate not empty
    TenantId-->>Client: Result<TenantId>.Success(tenantId)

    Client->>User: Create(tenantId, email, hash, name)
    User->>User: Validate inputs
    User->>User: new User(...)
    User->>User: AddDomainEvent(UserCreatedEvent)
    User-->>Client: Result<User>.Success(user)
```

### Repository Pattern Flow

```mermaid
sequenceDiagram
    participant Handler
    participant Repository
    participant UnitOfWork
    participant EventBus

    Handler->>Repository: AddAsync(user)
    Repository->>Repository: Track entity

    Handler->>UnitOfWork: SaveChangesAsync()
    UnitOfWork->>UnitOfWork: Save to database
    UnitOfWork->>EventBus: Dispatch domain events

    EventBus->>EventBus: Publish UserCreatedEvent

    UnitOfWork-->>Handler: Success
```

---

## Legend

### Diagram Symbols

| Symbol | Meaning |
|--------|---------|
| `<\|--` | Inheritance (is-a) |
| `*--` | Composition (owns) |
| `o--` | Aggregation (has-a) |
| `-->` | Association (uses) |
| `..>` | Dependency (depends on) |
| `<\|..` | Interface implementation |
| `<<abstract>>` | Abstract class |
| `<<interface>>` | Interface |
| `<<record>>` | Record type (C# 9+) |
| `$` | Static member |
| `*` | Abstract member |
| `#` | Protected member |
| `+` | Public member |
| `-` | Private member |

---

## Key Insights from Diagrams

### 1. **Clear Hierarchy**
- Everything starts with `BaseEntity` or `ValueObject`
- `AggregateRoot` adds event collection
- Entities inherit from `AggregateRoot`

### 2. **Strong Typing**
- Value objects provide type safety
- `Email`, `TenantId` prevent primitive obsession
- Compile-time guarantees

### 3. **Event-Driven**
- Aggregates collect events
- Events are published on save
- Loose coupling between components

### 4. **Result Pattern**
- No exceptions for business failures
- Explicit error handling
- Railway-oriented programming

### 5. **Clean Boundaries**
- Domain has no infrastructure dependencies
- Repositories are interfaces
- Services are abstractions

---

## Related Documentation

- [BaseEntity](./BaseEntity.md) - Entity base class
- [AggregateRoot](./AggregateRoot.md) - Aggregate pattern
- [ValueObject](./ValueObject.md) - Value object pattern
- [Result](./Result.md) - Result pattern
- [DomainEntities](./DomainEntities.md) - Entity details
- [ValueObjects](./ValueObjects.md) - Value object details
- [DomainEvents](./DomainEvents.md) - Event details
- [Phase2-Overview](./Phase2-Overview.md) - Complete overview

---

**Last Updated:** April 02, 2026  
**Diagram Count:** 12 comprehensive diagrams  
**Coverage:** 100% of domain layer concepts
