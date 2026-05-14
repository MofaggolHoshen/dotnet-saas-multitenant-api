# 1) DbContext and Dependency Injection

## Definition

`ApplicationDbContext` is the EF Core session for the Infrastructure layer. It maps domain entities to database tables and controls query/write behavior.

## Reason

A dedicated DbContext and DI registration are required to:

- isolate EF Core usage to Infrastructure
- apply cross-cutting data rules consistently
- keep startup composition centralized

## What was done

- Exposed EF sets for `User`, `Tenant`, `Role`, and `Permission`
- Added global query filters:
  - soft-delete: `!IsDeleted`
  - tenant filter for tenant-owned entities
- Added `QueryIgnoringFilters<T>()` for controlled bypass scenarios
- Registered DbContext with `UseNpgsql(...)`
- Registered interceptors and fallback unresolved tenant context
- Registered repositories and Unit of Work through `AddInfrastructureServices(...)`

## Pros

- One authoritative persistence entry point
- Consistent multi-tenant and soft-delete behavior
- Cleaner `Program.cs` and consistent composition root

## Cons

- Global filters can make debugging harder when data is unexpectedly hidden
- DI setup becomes more sensitive to registration order and lifetime choices

## Notes

The fallback unresolved tenant context is a temporary bridge for pre-Phase 5 flow and design-time operations.
