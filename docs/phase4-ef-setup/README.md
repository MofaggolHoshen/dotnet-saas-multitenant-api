# Phase 4: Infrastructure - EF Core Setup

This folder documents the EF Core infrastructure completed in Phase 4.

## Definition

Phase 4 establishes the persistence foundation of the system using EF Core and PostgreSQL in the Infrastructure layer, while keeping Domain and Application clean from EF-specific concerns.

## Why this phase exists

- Persist domain entities reliably
- Enforce tenant and soft-delete rules at query level
- Centralize transaction and repository behavior
- Enable schema evolution through migrations

## What was implemented

- PostgreSQL EF Core integration in Infrastructure
- `ApplicationDbContext` with tenant-aware and soft-delete query filters
- Fluent API entity configurations for domain entities
- SaveChanges interceptors for auditing and soft delete
- Repository implementations and Unit of Work
- Infrastructure dependency injection wiring
- Initial migration generation (`InitialCreate`)

## Benefits (Pros)

- Clear separation of concerns (Clean Architecture)
- Consistent query behavior via global filters
- Explicit mapping reduces accidental schema drift
- Easier testing and maintenance through abstractions

## Trade-offs (Cons)

- Added complexity (more classes and configuration)
- Query filters can hide data unless intentionally bypassed
- Repository + Unit of Work introduces extra abstraction layer

## Document index

- [1-DbContext-and-DI.md](./1-DbContext-and-DI.md)
- [2-Entity-Configurations.md](./2-Entity-Configurations.md)
- [3-Interceptors-Repositories-UoW.md](./3-Interceptors-Repositories-UoW.md)
- [4-Migrations-and-Validation.md](./4-Migrations-and-Validation.md)

## Key files

- `source/Infrastructure/Persistence/ApplicationDbContext.cs`
- `source/Infrastructure/Persistence/Configurations/*`
- `source/Infrastructure/Persistence/Interceptors/*`
- `source/Infrastructure/Persistence/Repositories/*`
- `source/Infrastructure/Persistence/UnitOfWork.cs`
- `source/Infrastructure/DependencyInjection.cs`
- `source/WebAPI/Program.cs`
- `source/WebAPI/appsettings.json`
