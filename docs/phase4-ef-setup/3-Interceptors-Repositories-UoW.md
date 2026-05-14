# 3) Interceptors, Repositories, and Unit of Work

## Definition

- **Interceptors**: hooks that run around EF save operations
- **Repositories**: Infrastructure implementations of Domain persistence contracts
- **Unit of Work**: transaction boundary and commit coordination

## Reason

These patterns keep data rules centralized and prevent business/application layers from directly depending on EF Core internals.

## What was implemented

### Interceptors

- `AuditableEntityInterceptor`
  - runs during SaveChanges
  - updates `UpdatedAtUtc` for added/modified entities
- `SoftDeleteInterceptor`
  - converts hard deletes into soft deletes
  - sets entity state to modified and applies `SoftDelete()`

### Repositories

- `UserRepository`
- `TenantRepository`
- `RoleRepository`

### Unit of Work

`UnitOfWork` wraps transaction operations:

- `SaveChangesAsync`
- `BeginTransactionAsync`
- `CommitTransactionAsync`
- `RollbackTransactionAsync`

## Pros

- Centralized persistence behavior
- Better testability through interfaces
- Explicit transaction handling for write operations

## Cons

- Additional abstraction over EF can duplicate built-in capabilities
- More layers may increase implementation overhead

## Notes

In this architecture, repositories remain thin and defer aggregate business rules to Domain objects.
