# Infrastructure Layer

The Infrastructure layer provides implementations for external concerns such as database access, external services, and identity management.

## Purpose

This layer implements interfaces defined in the Application and Domain layers. It handles all external I/O operations and infrastructure-specific concerns.

## Structure

```
Infrastructure/
├── Persistence/
│   ├── Configurations/    # EF Core entity configurations
│   ├── Interceptors/      # EF Core interceptors (auditing, soft delete, etc.)
│   ├── Repositories/      # Repository implementations
│   └── Migrations/        # EF Core database migrations
├── Identity/              # JWT authentication, password hashing
├── Multitenancy/          # Tenant resolution, tenant context
├── Caching/               # Redis caching implementation
└── Services/              # External service implementations
```

## Key Components

### Persistence
- **ApplicationDbContext**: Main EF Core database context
- **Entity Configurations**: Fluent API configurations for entities
- **Repositories**: Implementation of domain repository interfaces
- **Interceptors**: Cross-cutting concerns like auditing

### Identity
- **JwtService**: JWT token generation and validation
- **PasswordHasher**: Secure password hashing with BCrypt

### Multi-Tenancy
- **TenantResolver**: Resolves current tenant from request
- **TenantContext**: Provides access to current tenant info
- **TenantDbContextFactory**: Creates tenant-specific database contexts

### Caching
- **RedisCacheService**: Distributed caching implementation

## Dependencies

- **Application** - References application interfaces
- **Entity Framework Core** - ORM and data access
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **BCrypt.Net-Next** - Password hashing
- **StackExchange.Redis** - Redis client

## Guidelines

- Implement interfaces from Domain and Application layers
- Keep infrastructure concerns isolated
- Use dependency injection for flexibility
- Configure services in DependencyInjection extension class
