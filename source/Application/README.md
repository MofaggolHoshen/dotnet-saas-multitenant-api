# Application Layer

The Application layer orchestrates the flow of data between the Domain and Infrastructure layers. It implements the CQRS pattern using MediatR for handling commands and queries.

## Purpose

This layer contains application-specific business rules, use cases, and orchestration logic. It coordinates domain objects to perform application tasks without containing business rules themselves.

## Structure

```
Application/
├── Common/
│   ├── Behaviors/        # MediatR pipeline behaviors (validation, logging, etc.)
│   ├── Interfaces/       # Application service interfaces
│   ├── Mappings/         # AutoMapper profiles
│   └── Models/           # DTOs and response models
└── Features/
    ├── Auth/             # Authentication feature
    │   ├── Commands/     # Login, Register, RefreshToken, etc.
    │   └── Queries/      # GetCurrentUser, ValidateToken, etc.
    ├── Users/            # User management feature
    │   ├── Commands/     # CreateUser, UpdateUser, DeleteUser, etc.
    │   └── Queries/      # GetUser, GetUsers, etc.
    ├── Tenants/          # Tenant management feature
    │   ├── Commands/     # CreateTenant, UpdateTenant, etc.
    │   └── Queries/      # GetTenant, GetTenants, etc.
    └── Roles/            # Role and permission management
        ├── Commands/     # CreateRole, AssignPermission, etc.
        └── Queries/      # GetRole, GetPermissions, etc.
```

## CQRS Pattern

### Commands
- Represent intentions to change state
- Have handlers that execute the business logic
- Return result indicating success/failure
- Validated using FluentValidation

### Queries
- Represent requests for data
- Should be side-effect free
- Return DTOs, not domain entities

## Dependencies

- **Domain** - References domain entities and repository interfaces
- **MediatR** - CQRS and mediator pattern
- **FluentValidation** - Request validation
- **AutoMapper** - Object mapping between layers

## Guidelines

- Keep handlers focused and single-purpose
- Use pipeline behaviors for cross-cutting concerns
- Validate all incoming requests
- Return DTOs, never expose domain entities directly
