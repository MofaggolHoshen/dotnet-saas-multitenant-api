# Phase 3: Application Layer Setup (MediatR)

This document explains the full Phase 3 implementation in the `Application` layer, including all steps, classes, interfaces, models, mappings, exceptions, package setup, and folder conventions.

---

## 1) Goal of Phase 3

Phase 3 establishes the **Application layer foundation** for Clean Architecture + CQRS:

- Register Application services in one place
- Introduce CQRS contracts (`ICommand`, `IQuery`)
- Keep Application independent from infrastructure concerns
- Add common models used by handlers and API responses
- Add application-specific exceptions
- Add AutoMapper mapping conventions
- Prepare feature-folder structure for vertical slices

---

## 2) Package Setup (`source/Application/Application.csproj`)

The following packages are used:

- `MediatR` (14.1.0)
  - Enables mediator/CQRS request-handler pattern.
- `FluentValidation` (12.1.1)
  - Defines validators for commands/queries.
- `FluentValidation.DependencyInjectionExtensions` (12.1.1)
  - Enables assembly scanning registration (`AddValidatorsFromAssembly`).
- `AutoMapper` (12.0.1)
  - Object-object mapping.
- `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.1)
  - DI registration integration (`AddAutoMapper`).

Also references `Domain` project:

- `ProjectReference ..\Domain\Domain.csproj`

---

## 3) Step-by-Step Implementation

## Day 1: MediatR and Core Interfaces

### Step 1. Create `DependencyInjection.cs`
**File:** `source/Application/DependencyInjection.cs`

What it does:

- Gets current assembly with `Assembly.GetExecutingAssembly()`
- Registers MediatR handlers from Application assembly
- Registers FluentValidation validators from Application assembly
- Registers AutoMapper profiles from Application assembly

Why it matters:

- Single entry point to wire all Application services
- Keeps `Program.cs` clean in API layer
- Ensures all handlers/validators/mappings are auto-discovered

---

### Step 2. Add CQRS marker interfaces

#### `ICommand<TResponse>`
**File:** `source/Application/Common/Interfaces/ICommand.cs`

- Inherits `IRequest<TResponse>` from MediatR
- Represents **write operations** (state changes)

#### `IQuery<TResponse>`
**File:** `source/Application/Common/Interfaces/IQuery.cs`

- Inherits `IRequest<TResponse>` from MediatR
- Represents **read operations** (no side effects)

Why they matter:

- Clear semantic separation between read and write flows
- Better consistency and discoverability in feature folders

---

### Step 3. Add application abstractions (interfaces)

#### `IApplicationDbContext`
**File:** `source/Application/Common/Interfaces/IApplicationDbContext.cs`

- Exposes:
  - `IQueryable<User> Users`
  - `IQueryable<Tenant> Tenants`
  - `IQueryable<Role> Roles`
  - `IQueryable<Permission> Permissions`
  - `Task<int> SaveChangesAsync(...)`

Important design decision:

- Uses `IQueryable<T>` (not `DbSet<T>`) to avoid EF Core dependency leaking into Application.
- Infrastructure will implement this interface using EF Core.

#### `ICurrentUserService`
**File:** `source/Application/Common/Interfaces/ICurrentUserService.cs`

Exposes current authenticated user context:

- `UserId`
- `Email`
- `TenantId`
- `IsAuthenticated`

#### `ITenantContext`
**File:** `source/Application/Common/Interfaces/ITenantContext.cs`

Exposes resolved tenant context:

- `TenantId`
- `TenantName`
- `IsResolved`

#### `IDateTime`
**File:** `source/Application/Common/Interfaces/IDateTime.cs`

- Exposes `DateTime UtcNow`
- Enables deterministic tests by allowing fake clock implementations

---

## Day 2: Common Models and Exceptions

### Step 4. Add pagination model

#### `PaginatedList<T>`
**File:** `source/Application/Common/Models/PaginatedList.cs`

Contains:

- `Items`
- `TotalCount`
- `PageNumber`
- `PageSize`
- `TotalPages`
- `HasPreviousPage`
- `HasNextPage`

Purpose:

- Standard pagination response structure across queries.

---

### Step 5. Add API response model

#### `ApiResponse<T>`
**File:** `source/Application/Common/Models/ApiResponse.cs`

Contains:

- `Success`
- `Data`
- `Message`
- `Errors`

Factory methods:

- `Ok(T data, string? message = null)`
- `Fail(string message, params string[] errors)`

Purpose:

- Consistent success/failure response envelope.

---

### Step 6. Add error DTOs

#### `ValidationError`
**File:** `source/Application/Common/Models/ValidationError.cs`

- Record containing: `PropertyName`, `ErrorMessage`

#### `ErrorDetails`
**File:** `source/Application/Common/Models/ErrorDetails.cs`

- Record containing: `Code`, `Message`, optional `TraceId`

Purpose:

- Structured error payloads for validation and runtime failures.

---

### Step 7. Add application exceptions

#### `ValidationException`
**File:** `source/Application/Common/Exceptions/ValidationException.cs`

- Wraps FluentValidation failures
- Exposes grouped error dictionary: `IReadOnlyDictionary<string, string[]> Errors`

#### `NotFoundException`
**File:** `source/Application/Common/Exceptions/NotFoundException.cs`

- Includes `EntityName` and `Key`
- Message format: `Entity "{name}" ({key}) was not found.`

#### `ForbiddenAccessException`
**File:** `source/Application/Common/Exceptions/ForbiddenAccessException.cs`

- Used for authorization failures
- Supports default/custom/inner-exception constructors

Purpose:

- Centralized application-level exception types for middleware handling.

---

## Day 3: AutoMapper and Feature Organization

### Step 8. Add mapping convention interfaces

#### `IMapFrom<T>`
**File:** `source/Application/Common/Mappings/IMapFrom.cs`

- Interface for DTOs that define their own mapping rules
- Default `Mapping(Profile profile)` implementation calls `CreateMap(typeof(T), GetType())`

Purpose:

- Keeps mapping configuration close to DTOs.

---

### Step 9. Add mapping profile discovery

#### `MappingProfile`
**File:** `source/Application/Common/Mappings/MappingProfile.cs`

- Scans assembly for types implementing `IMapFrom<>`
- Creates instances via reflection
- Invokes each type's `Mapping(...)` method

Purpose:

- Automatic mapping registration without manual per-DTO profile wiring.

---

### Step 10. Set up feature folder structure

Prepared vertical-slice folders with `.gitkeep` files:

- `Features/Auth/Commands`, `Features/Auth/Queries`
- `Features/Users/Commands`, `Features/Users/Queries`
- `Features/Tenants/Commands`, `Features/Tenants/Queries`
- `Features/Roles/Commands`, `Features/Roles/Queries`

Purpose:

- Enforces a scalable CQRS-by-feature organization.

---

### Step 11. Document conventions

- Updated `source/Application/README.md` with:
  - Structure rules
  - Naming conventions
  - Command/query examples
  - Handler and validator guidance
  - Best practices

---

## 4) Architectural Rationale

Key principles applied in Phase 3:

1. **Application is framework-light and infrastructure-agnostic**
   - No direct EF Core dependency in contracts.
2. **CQRS separation is explicit**
   - Commands and queries use separate marker interfaces.
3. **Cross-cutting registration is centralized**
   - MediatR, validators, AutoMapper configured from one entry point.
4. **Conventions over manual boilerplate**
   - Assembly scanning for handlers/validators/mappings.
5. **Future-ready for pipeline behaviors**
   - `DependencyInjection.cs` includes placeholders for behavior registration.

---

## 5) File Inventory (Phase 3)

### Service Registration
- `source/Application/DependencyInjection.cs`

### Interfaces
- `source/Application/Common/Interfaces/ICommand.cs`
- `source/Application/Common/Interfaces/IQuery.cs`
- `source/Application/Common/Interfaces/IApplicationDbContext.cs`
- `source/Application/Common/Interfaces/ICurrentUserService.cs`
- `source/Application/Common/Interfaces/ITenantContext.cs`
- `source/Application/Common/Interfaces/IDateTime.cs`

### Models
- `source/Application/Common/Models/PaginatedList.cs`
- `source/Application/Common/Models/ApiResponse.cs`
- `source/Application/Common/Models/ValidationError.cs`
- `source/Application/Common/Models/ErrorDetails.cs`

### Exceptions
- `source/Application/Common/Exceptions/ValidationException.cs`
- `source/Application/Common/Exceptions/NotFoundException.cs`
- `source/Application/Common/Exceptions/ForbiddenAccessException.cs`

### Mappings
- `source/Application/Common/Mappings/IMapFrom.cs`
- `source/Application/Common/Mappings/MappingProfile.cs`

### Feature Structure Placeholders
- `source/Application/Features/Auth/Commands/.gitkeep`
- `source/Application/Features/Auth/Queries/.gitkeep`
- `source/Application/Features/Users/Commands/.gitkeep`
- `source/Application/Features/Users/Queries/.gitkeep`
- `source/Application/Features/Tenants/Commands/.gitkeep`
- `source/Application/Features/Tenants/Queries/.gitkeep`
- `source/Application/Features/Roles/Commands/.gitkeep`
- `source/Application/Features/Roles/Queries/.gitkeep`

### Supporting Docs
- `source/Application/README.md`

---

## 6) How to Use This Foundation in Next Phases

When implementing a new use case:

1. Create a feature folder path under `Features/[Feature]/Commands|Queries/[UseCase]`.
2. Create request model implementing `ICommand<T>` or `IQuery<T>`.
3. Add FluentValidation validator.
4. Implement MediatR handler.
5. Return DTOs/models, not domain entities directly.
6. If needed, implement `IMapFrom<T>` in DTO and customize `Mapping`.
7. Depend on `IApplicationDbContext`, `ICurrentUserService`, `ITenantContext`, `IDateTime` abstractions.

---

## 7) Completion State

Phase 3 is implemented and build-validated, providing a complete Application-layer base for upcoming phases (Infrastructure, pipelines, and concrete feature handlers).
