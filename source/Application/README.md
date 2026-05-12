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

## CQRS Folder Conventions

### Feature Organization
Each feature is organized by use case, not by CRUD operations. Features follow a consistent structure:

```
Features/
└── [FeatureName]/
    ├── Commands/
    │   └── [UseCase]/
    │       ├── [UseCase]Command.cs      # Request object implementing ICommand<TResponse>
    │       ├── [UseCase]Validator.cs     # FluentValidation validator
    │       ├── [UseCase]Handler.cs       # Command handler implementing IRequestHandler
    │       └── [UseCase]Response.cs      # Response DTO (optional)
    └── Queries/
        └── [UseCase]/
            ├── [UseCase]Query.cs         # Request object implementing IQuery<TResponse>
            ├── [UseCase]Validator.cs     # FluentValidation validator
            ├── [UseCase]Handler.cs       # Query handler implementing IRequestHandler
            └── [UseCase]Response.cs      # Response DTO
```

### Naming Conventions
- **Commands**: Use imperative verbs (CreateUser, UpdateTenant, DeleteRole)
- **Queries**: Use "Get" prefix (GetUser, GetUsers, GetTenantById)
- **Handlers**: Suffix with "Handler" (CreateUserHandler, GetUsersHandler)
- **Validators**: Suffix with "Validator" (CreateUserValidator)
- **Responses**: Suffix with "Response" or "Dto" (UserResponse, UserDto)

### Examples

#### Command Example
```csharp
// CreateUser/CreateUserCommand.cs
public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FullName
) : ICommand<Guid>;

// CreateUser/CreateUserValidator.cs
public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
    }
}

// CreateUser/CreateUserHandler.cs
public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateUserHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Orchestration logic here
        // Delegate business rules to Domain entities
    }
}
```

#### Query Example
```csharp
// GetUsers/GetUsersQuery.cs
public sealed record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IQuery<PaginatedList<UserDto>>;

// GetUsers/GetUsersHandler.cs
public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        // Query and map to DTOs
    }
}

// GetUsers/UserDto.cs
public sealed record UserDto : IMapFrom<User>
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value));
    }
}
```

### Best Practices

1. **One File Per Concern**: Keep command, validator, handler, and response in separate files
2. **Thin Handlers**: Handlers should orchestrate, not contain business logic
3. **Domain-First**: Business rules belong in Domain entities, not handlers
4. **No Direct Entity Exposure**: Always return DTOs from handlers
5. **Colocated Mappings**: Define AutoMapper mappings on DTOs using `IMapFrom<T>`
6. **Validation First**: Always validate requests before processing
7. **Use Result Pattern**: Return `Result<T>` from Domain, map to responses in handlers
8. **Async All The Way**: Use async/await for all database and external operations

## Guidelines

- Keep handlers focused and single-purpose
- Use pipeline behaviors for cross-cutting concerns
- Validate all incoming requests
- Return DTOs, never expose domain entities directly
- Follow the established folder structure for consistency
- Document complex use cases with XML comments
