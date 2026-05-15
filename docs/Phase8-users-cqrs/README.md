# Phase 8 — Users Feature: CQRS Implementation

**Phase**: 8  
**Status**: ⚪ Not Started  
**Depends On**: Phase 7 (Auth Feature - CQRS Implementation)  
**Duration**: 5 days

---

## Overview

Phase 8 builds a complete user management feature using CQRS patterns established in Phase 3. Every user operation (create, update, delete, role assignment, and queries) is implemented as a dedicated MediatR `Command` or `Query` with its own `Validator` and `Handler`. This phase extends the authentication foundation from Phase 7 with full CRUD capabilities, pagination, filtering, and role management.

The user feature demonstrates Clean Architecture principles: thin controllers delegate to application handlers, which orchestrate domain entities and infrastructure repositories, while maintaining strict tenant isolation.

---

## Files Added / Modified

### Commands

| Action   | File                                                                           | Description                                           |
| -------- | ------------------------------------------------------------------------------ | ----------------------------------------------------- |
| ➕ Added | `Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs`          | Command record for user creation with role assignment |
| ➕ Added | `Application/Features/Users/Commands/CreateUser/CreateUserCommandValidator.cs` | Validates email, password strength, full name         |
| ➕ Added | `Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`   | Creates tenant-scoped user with initial roles         |
| ➕ Added | `Application/Features/Users/Commands/UpdateUser/UpdateUserCommand.cs`          | Command record for updating user profile              |
| ➕ Added | `Application/Features/Users/Commands/UpdateUser/UpdateUserCommandValidator.cs` | Validates update fields                               |
| ➕ Added | `Application/Features/Users/Commands/UpdateUser/UpdateUserCommandHandler.cs`   | Updates user while preserving invariants              |
| ➕ Added | `Application/Features/Users/Commands/DeleteUser/DeleteUserCommand.cs`          | Command record for soft delete                        |
| ➕ Added | `Application/Features/Users/Commands/DeleteUser/DeleteUserCommandValidator.cs` | Validates user ID                                     |
| ➕ Added | `Application/Features/Users/Commands/DeleteUser/DeleteUserCommandHandler.cs`   | Soft deletes user for auditability                    |
| ➕ Added | `Application/Features/Users/Commands/AssignRole/AssignRoleCommand.cs`          | Command record for role assignment                    |
| ➕ Added | `Application/Features/Users/Commands/AssignRole/AssignRoleCommandValidator.cs` | Validates user and role IDs                           |
| ➕ Added | `Application/Features/Users/Commands/AssignRole/AssignRoleCommandHandler.cs`   | Assigns role to user (idempotent)                     |
| ➕ Added | `Application/Features/Users/Commands/RemoveRole/RemoveRoleCommand.cs`          | Command record for role removal                       |
| ➕ Added | `Application/Features/Users/Commands/RemoveRole/RemoveRoleCommandValidator.cs` | Validates user and role IDs                           |
| ➕ Added | `Application/Features/Users/Commands/RemoveRole/RemoveRoleCommandHandler.cs`   | Removes role from user (idempotent)                   |

### Queries

| Action   | File                                                                            | Description                                         |
| -------- | ------------------------------------------------------------------------------- | --------------------------------------------------- |
| ➕ Added | `Application/Features/Users/Queries/GetUsers/GetUsersQuery.cs`                  | Query record for paginated user list with filtering |
| ➕ Added | `Application/Features/Users/Queries/GetUsers/GetUsersQueryValidator.cs`         | Validates pagination parameters                     |
| ➕ Added | `Application/Features/Users/Queries/GetUsers/GetUsersQueryHandler.cs`           | Returns tenant-scoped paginated user list           |
| ➕ Added | `Application/Features/Users/Queries/GetUserById/GetUserByIdQuery.cs`            | Query record for single user details                |
| ➕ Added | `Application/Features/Users/Queries/GetUserById/GetUserByIdQueryValidator.cs`   | Validates user ID                                   |
| ➕ Added | `Application/Features/Users/Queries/GetUserById/GetUserByIdQueryHandler.cs`     | Returns detailed user information                   |
| ➕ Added | `Application/Features/Users/Queries/GetUserRoles/GetUserRolesQuery.cs`          | Query record for user's roles                       |
| ➕ Added | `Application/Features/Users/Queries/GetUserRoles/GetUserRolesQueryValidator.cs` | Validates user ID                                   |
| ➕ Added | `Application/Features/Users/Queries/GetUserRoles/GetUserRolesQueryHandler.cs`   | Returns list of roles assigned to user              |

### DTOs

| Action   | File                                               | Description                          |
| -------- | -------------------------------------------------- | ------------------------------------ |
| ➕ Added | `Application/Features/Users/DTOs/UserDto.cs`       | Lightweight user data for list views |
| ➕ Added | `Application/Features/Users/DTOs/UserDetailDto.cs` | Complete user data for detail views  |
| ➕ Added | `Application/Features/Users/DTOs/UserRoleDto.cs`   | Role information for user context    |

### Mappings

| Action   | File                                                        | Description                                      |
| -------- | ----------------------------------------------------------- | ------------------------------------------------ |
| ➕ Added | `Application/Features/Users/Mappings/UserMappingProfile.cs` | AutoMapper configuration for User → DTO mappings |

### Repository Extensions

| Action      | File                                                        | Description                         |
| ----------- | ----------------------------------------------------------- | ----------------------------------- |
| 🔧 Modified | `Domain/Repositories/IUserRepository.cs`                    | Added pagination and search methods |
| 🔧 Modified | `Infrastructure/Persistence/Repositories/UserRepository.cs` | Implemented pagination and search   |

---

## Architecture Decisions

### 1. Read Models vs Domain Entities

Commands work with domain entities (`User`, `Role`), while queries return DTOs (`UserDto`, `UserDetailDto`). This separation keeps the API contract stable even when domain models evolve.

**User Entity (Write Model):**

```csharp
public class User : AggregateRoot
{
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    // ... internal domain behavior
}
```

**UserDto (Read Model):**

```csharp
public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAtUtc
);
```

The handler translates between these using AutoMapper:

```csharp
var userDto = _mapper.Map<UserDto>(user);
```

### 2. Tenant Isolation in Queries

All queries automatically filter by `ITenantContext.TenantId`. Users can never query across tenant boundaries:

```csharp
var users = await _dbContext.UsersSet
    .Where(u => u.TenantId == _tenantContext.TenantId)
    .Where(u => !u.IsDeleted)
    .ToListAsync();
```

This is enforced at the application layer, making it impossible for a controller to accidentally leak data.

### 3. Soft Delete Strategy

`DeleteUserCommand` marks users as deleted (`IsDeleted = true`) instead of removing the row. This preserves audit trails and foreign key integrity:

```csharp
user.MarkAsDeleted();  // Sets IsDeleted = true, UpdatedAtUtc = now
await _unitOfWork.SaveChangesAsync();
```

All queries automatically exclude soft-deleted users via the `IsDeleted` filter.

### 4. Idempotent Role Assignment

`AssignRoleCommand` and `RemoveRoleCommand` are idempotent — calling them multiple times produces the same result as calling once:

```csharp
// AssignRole
if (!user.RoleIds.Contains(roleId))
{
    user.AssignRole(roleId);
}

// RemoveRole
if (user.RoleIds.Contains(roleId))
{
    user.RemoveRole(roleId);
}
```

This makes the API safe for retry scenarios and simplifies client logic.

### 5. Pagination with Sensible Defaults

`GetUsersQuery` enforces pagination to prevent large result sets from overwhelming the API or database:

| Parameter    | Default | Min | Max   | Purpose                      |
| ------------ | ------- | --- | ----- | ---------------------------- |
| `PageNumber` | `1`     | `1` | ∞     | Current page (1-indexed)     |
| `PageSize`   | `10`    | `1` | `100` | Items per page               |
| `Search`     | `null`  | -   | -     | Filter by email or full name |
| `IsActive`   | `null`  | -   | -     | Filter by active status      |

Example request:

```http
GET /api/v1/users?pageNumber=2&pageSize=25&search=alice&isActive=true
```

### 6. Eager Loading to Prevent N+1 Queries

User queries always include roles using `.Include()`:

```csharp
var users = await _dbContext.UsersSet
    .Include(u => u.Roles)  // Single JOIN instead of N+1 queries
    .Where(u => u.TenantId == _tenantContext.TenantId)
    .ToListAsync();
```

This loads all related data in one database round-trip.

---

## Command Flow Diagrams

### CreateUser Flow

```
POST /api/v1/users
  │
  ├─ CreateUserCommandValidator (FluentValidation via MediatR pipeline)
  │    ├─ Email: not empty + valid format
  │    ├─ Password: not empty + PasswordValidator (strength rules)
  │    ├─ FullName: not empty, 2–120 chars
  │    └─ RoleIds: not null (can be empty list)
  │
  └─ CreateUserCommandHandler
       ├─ ITenantContext.IsResolved? → else: Validation error
       ├─ TenantId.Create(tenantContext.TenantId)
       ├─ Email.Create(request.Email)
       ├─ IUserRepository.GetByEmailAsync(tenantId, email)
       │    └─ if exists → Conflict error
       ├─ IPasswordHashingService.Hash(request.Password)
       ├─ User.Create(tenantId, email, hash, fullName)
       ├─ foreach roleId in request.RoleIds:
       │    ├─ IRoleRepository.GetByIdAsync(roleId)
       │    └─ user.AssignRole(roleId)
       ├─ IUserRepository.AddAsync(user)
       ├─ IUnitOfWork.SaveChangesAsync()
       └─ Result.Success(user.Id)
```

### UpdateUser Flow

```
PUT /api/v1/users/{id}
  │
  ├─ UpdateUserCommandValidator
  │    ├─ UserId: not empty
  │    └─ At least one field to update
  │
  └─ UpdateUserCommandHandler
       ├─ ITenantContext.IsResolved?
       ├─ IUserRepository.GetByIdAsync(userId)
       │    └─ if not found or wrong tenant → NotFound error
       ├─ if (request.FullName != null):
       │    └─ user.UpdateFullName(request.FullName)
       ├─ if (request.IsActive != null):
       │    └─ user.SetActiveStatus(request.IsActive.Value)
       ├─ IUnitOfWork.SaveChangesAsync()
       └─ Result.Success()
```

### DeleteUser Flow (Soft Delete)

```
DELETE /api/v1/users/{id}
  │
  ├─ DeleteUserCommandValidator
  │    └─ UserId: not empty
  │
  └─ DeleteUserCommandHandler
       ├─ ITenantContext.IsResolved?
       ├─ IUserRepository.GetByIdAsync(userId)
       │    └─ if not found or wrong tenant → NotFound error
       ├─ user.MarkAsDeleted()  // Sets IsDeleted = true
       ├─ IRefreshTokenRepository.RevokeAllUserTokensAsync(userId)
       ├─ IUnitOfWork.SaveChangesAsync()
       └─ Result.Success()
```

### AssignRole Flow

```
POST /api/v1/users/{userId}/roles/{roleId}
  │
  ├─ AssignRoleCommandValidator
  │    ├─ UserId: not empty
  │    └─ RoleId: not empty
  │
  └─ AssignRoleCommandHandler
       ├─ ITenantContext.IsResolved?
       ├─ IUserRepository.GetByIdAsync(userId)
       │    └─ if not found or wrong tenant → NotFound error
       ├─ IRoleRepository.GetByIdAsync(roleId)
       │    └─ if not found or wrong tenant → NotFound error
       ├─ if (!user.RoleIds.Contains(roleId)):
       │    └─ user.AssignRole(roleId)
       ├─ IUnitOfWork.SaveChangesAsync()
       └─ Result.Success()
```

### GetUsers Flow (Paginated Query)

```
GET /api/v1/users?pageNumber=1&pageSize=10&search=alice&isActive=true
  │
  ├─ GetUsersQueryValidator
  │    ├─ PageNumber: >= 1
  │    ├─ PageSize: 1–100
  │    └─ Search: max 100 chars (if provided)
  │
  └─ GetUsersQueryHandler
       ├─ ITenantContext.IsResolved?
       ├─ Build query:
       │    ├─ Base: _dbContext.UsersSet
       │    │        .Include(u => u.Roles)
       │    │        .Where(u => u.TenantId == tenantId)
       │    │        .Where(u => !u.IsDeleted)
       │    ├─ if (Search != null):
       │    │    └─ .Where(u => u.Email.Contains(search) || u.FullName.Contains(search))
       │    └─ if (IsActive != null):
       │         └─ .Where(u => u.IsActive == isActive)
       ├─ Execute paginated query:
       │    ├─ Total count (before pagination)
       │    ├─ Skip((pageNumber - 1) * pageSize)
       │    ├─ Take(pageSize)
       │    └─ .ToListAsync()
       ├─ Map to DTOs: _mapper.Map<List<UserDto>>(users)
       └─ Return PaginatedList<UserDto> { Items, TotalCount, PageNumber, PageSize }
```

### GetUserById Flow

```
GET /api/v1/users/{id}
  │
  ├─ GetUserByIdQueryValidator
  │    └─ UserId: not empty
  │
  └─ GetUserByIdQueryHandler
       ├─ ITenantContext.IsResolved?
       ├─ IUserRepository.GetByIdAsync(userId)
       │    └─ Include roles: .Include(u => u.Roles)
       ├─ if (user == null || user.TenantId != tenantId || user.IsDeleted):
       │    └─ NotFound error
       ├─ _mapper.Map<UserDetailDto>(user)
       └─ Result.Success(userDetailDto)
```

---

## Validation Rules Summary

| Command/Query | Field      | Rules                                                                 |
| ------------- | ---------- | --------------------------------------------------------------------- |
| CreateUser    | Email      | NotEmpty, EmailAddress, MaxLength(256)                                |
| CreateUser    | Password   | NotEmpty, MinLength(8), PasswordValidator (upper/lower/digit/special) |
| CreateUser    | FullName   | NotEmpty, Length(2, 120)                                              |
| CreateUser    | RoleIds    | NotNull (can be empty list)                                           |
| UpdateUser    | UserId     | NotEmpty                                                              |
| UpdateUser    | FullName   | Optional, Length(2, 120) when provided                                |
| UpdateUser    | IsActive   | Optional, boolean when provided                                       |
| DeleteUser    | UserId     | NotEmpty                                                              |
| AssignRole    | UserId     | NotEmpty                                                              |
| AssignRole    | RoleId     | NotEmpty                                                              |
| RemoveRole    | UserId     | NotEmpty                                                              |
| RemoveRole    | RoleId     | NotEmpty                                                              |
| GetUsers      | PageNumber | GreaterThanOrEqualTo(1)                                               |
| GetUsers      | PageSize   | InclusiveBetween(1, 100)                                              |
| GetUsers      | Search     | Optional, MaxLength(100)                                              |
| GetUsers      | IsActive   | Optional, boolean                                                     |
| GetUserById   | UserId     | NotEmpty                                                              |
| GetUserRoles  | UserId     | NotEmpty                                                              |

---

## DTOs Structure

### UserDto (List View)

Used in `GetUsersQuery` for lightweight list representations.

```csharp
public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAtUtc
);
```

**Use Cases:**

- User management list pages
- Dropdown/autocomplete selections
- Audit logs

---

### UserDetailDto (Detail View)

Used in `GetUserByIdQuery` for comprehensive user information.

```csharp
public sealed record UserDetailDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<UserRoleDto> Roles
);
```

**Use Cases:**

- User profile pages
- Admin user detail views
- Full user context for reports

---

### UserRoleDto

Used to represent role membership within user DTOs.

```csharp
public sealed record UserRoleDto(
    Guid Id,
    string Name,
    string? Description
);
```

**Use Cases:**

- Displaying user permissions
- Role assignment UI
- Access control contexts

---

## AutoMapper Configuration

**File**: `Application/Features/Users/Mappings/UserMappingProfile.cs`

Maps domain entities to DTOs using AutoMapper profiles:

```csharp
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User → UserDto (list view)
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.CreatedAtUtc));

        // User → UserDetailDto (detail view)
        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));

        // Role → UserRoleDto
        CreateMap<Role, UserRoleDto>();
    }
}
```

**Registration** (in `Application/DependencyInjection.cs`):

```csharp
services.AddAutoMapper(typeof(UserMappingProfile).Assembly);
```

---

## Repository Extensions

### IUserRepository Interface (Domain Layer)

**File**: `Domain/Repositories/IUserRepository.cs`

New methods added for pagination and search:

```csharp
public interface IUserRepository
{
    // Existing methods from Phase 2
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(TenantId tenantId, Email email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);

    // Phase 8 additions
    Task<(List<User> Users, int TotalCount)> GetPaginatedAsync(
        TenantId tenantId,
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(
        TenantId tenantId,
        string? searchTerm = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}
```

---

### UserRepository Implementation (Infrastructure Layer)

**File**: `Infrastructure/Persistence/Repositories/UserRepository.cs`

```csharp
public async Task<(List<User> Users, int TotalCount)> GetPaginatedAsync(
    TenantId tenantId,
    int pageNumber,
    int pageSize,
    string? searchTerm = null,
    bool? isActive = null,
    CancellationToken cancellationToken = default)
{
    var query = _dbContext.UsersSet
        .Include(u => u.Roles)
        .Where(u => u.TenantId == tenantId)
        .Where(u => !u.IsDeleted)
        .AsQueryable();

    // Apply search filter
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var lowerSearch = searchTerm.ToLower();
        query = query.Where(u =>
            u.Email.Value.ToLower().Contains(lowerSearch) ||
            u.FullName.ToLower().Contains(lowerSearch));
    }

    // Apply active filter
    if (isActive.HasValue)
    {
        query = query.Where(u => u.IsActive == isActive.Value);
    }

    var totalCount = await query.CountAsync(cancellationToken);

    var users = await query
        .OrderByDescending(u => u.CreatedAtUtc)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return (users, totalCount);
}
```

**Performance optimizations:**

- Single database query using `.Include()` for roles
- `.CountAsync()` before pagination for total count
- `.OrderByDescending()` for consistent ordering
- Index on `tenant_id`, `is_deleted`, and `created_at_utc` columns (existing from Phase 2)

---

## Interfaces Used by Handlers

| Interface                 | Layer       | Purpose                               |
| ------------------------- | ----------- | ------------------------------------- |
| `ICommand<TResponse>`     | Application | MediatR command marker                |
| `IQuery<TResponse>`       | Application | MediatR query marker                  |
| `ITenantContext`          | Application | Current tenant resolution             |
| `IUnitOfWork`             | Application | Persist changes atomically            |
| `ICurrentUserService`     | Application | Authenticated user's claims           |
| `IUserRepository`         | Domain      | User CRUD + pagination + search       |
| `IRoleRepository`         | Domain      | Role lookup and validation            |
| `IPasswordHashingService` | Domain      | Password hashing (used in CreateUser) |
| `IRefreshTokenRepository` | Application | Token revocation (used in DeleteUser) |
| `IMapper`                 | Application | Entity → DTO mapping (AutoMapper)     |

---

## Response Examples

### CreateUserCommand Response

**Request:**

```http
POST /api/v1/users
Content-Type: application/json

{
  "email": "alice@example.com",
  "password": "SecureP@ss123",
  "fullName": "Alice Johnson",
  "roleIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ]
}
```

**Success Response (201 Created):**

```json
{
  "success": true,
  "data": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "errors": null
}
```

**Validation Error (400 Bad Request):**

```json
{
  "success": false,
  "data": null,
  "errors": [
    "Email is required.",
    "Password must be at least 8 characters long.",
    "Password must contain at least one uppercase letter."
  ]
}
```

**Conflict Error (409 Conflict):**

```json
{
  "success": false,
  "data": null,
  "errors": ["A user with this email already exists."]
}
```

---

### GetUsersQuery Response

**Request:**

```http
GET /api/v1/users?pageNumber=1&pageSize=10&search=alice&isActive=true
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
        "email": "alice@example.com",
        "fullName": "Alice Johnson",
        "isActive": true,
        "createdAtUtc": "2026-05-15T10:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "errors": null
}
```

---

### GetUserByIdQuery Response

**Request:**

```http
GET /api/v1/users/9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "tenantId": "b1a5c9d2-0001-4f3e-9c1b-aabbccddeeff",
    "email": "alice@example.com",
    "fullName": "Alice Johnson",
    "isActive": true,
    "createdAtUtc": "2026-05-15T10:30:00Z",
    "updatedAtUtc": "2026-05-15T14:20:00Z",
    "roles": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Admin",
        "description": "Administrator role with full permissions"
      }
    ]
  },
  "errors": null
}
```

**Not Found Error (404 Not Found):**

```json
{
  "success": false,
  "data": null,
  "errors": ["User not found."]
}
```

---

## Testing Strategy

### Unit Tests

Test each handler in isolation using mocked dependencies:

**CreateUserCommandHandlerTests.cs:**

```csharp
[Fact]
public async Task Handle_ValidRequest_ReturnsSuccessWithUserId()
{
    // Arrange
    var userRepositoryMock = new Mock<IUserRepository>();
    var passwordHasherMock = new Mock<IPasswordHashingService>();
    var tenantContextMock = new Mock<ITenantContext>();

    tenantContextMock.Setup(x => x.IsResolved).Returns(true);
    tenantContextMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

    userRepositoryMock
        .Setup(x => x.GetByEmailAsync(It.IsAny<TenantId>(), It.IsAny<Email>(), default))
        .ReturnsAsync((User?)null); // Email not taken

    passwordHasherMock
        .Setup(x => x.Hash(It.IsAny<string>()))
        .Returns("hashed_password");

    var handler = new CreateUserCommandHandler(
        userRepositoryMock.Object,
        passwordHasherMock.Object,
        tenantContextMock.Object,
        unitOfWorkMock.Object);

    var command = new CreateUserCommand(
        "alice@example.com",
        "SecureP@ss123",
        "Alice Johnson",
        new List<Guid>());

    // Act
    var result = await handler.Handle(command, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEqual(Guid.Empty, result.Value);
    userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), default), Times.Once);
}
```

**GetUsersQueryHandlerTests.cs:**

```csharp
[Fact]
public async Task Handle_WithPagination_ReturnsCorrectPage()
{
    // Arrange
    var users = GenerateTestUsers(25); // Create 25 test users
    var userRepositoryMock = new Mock<IUserRepository>();

    userRepositoryMock
        .Setup(x => x.GetPaginatedAsync(
            It.IsAny<TenantId>(),
            2, // Page 2
            10, // Page size 10
            null,
            null,
            default))
        .ReturnsAsync((users.Skip(10).Take(10).ToList(), 25));

    var handler = new GetUsersQueryHandler(
        userRepositoryMock.Object,
        mapperMock.Object,
        tenantContextMock.Object);

    var query = new GetUsersQuery(PageNumber: 2, PageSize: 10);

    // Act
    var result = await handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(10, result.Value.Items.Count);
    Assert.Equal(25, result.Value.TotalCount);
    Assert.Equal(3, result.Value.TotalPages);
}
```

### Integration Tests

Test the full request pipeline with a real database (or test database):

```csharp
public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateUser_ValidData_Returns201()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new CreateUserCommand(
            "test@example.com",
            "SecureP@ss123",
            "Test User",
            new List<Guid>());

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, userId);
    }
}
```

---

## Performance Considerations

### 1. N+1 Query Prevention

Always use `.Include()` when loading related roles:

```csharp
// ❌ BAD: N+1 queries (1 for users + N for each user's roles)
var users = await _dbContext.UsersSet.ToListAsync();
foreach (var user in users)
{
    var roles = await _dbContext.RolesSet.Where(r => user.RoleIds.Contains(r.Id)).ToListAsync();
}

// ✅ GOOD: Single JOIN query
var users = await _dbContext.UsersSet
    .Include(u => u.Roles)
    .ToListAsync();
```

### 2. Pagination Enforcement

The `GetUsersQuery` validator enforces a maximum page size of 100 to prevent large result sets:

```csharp
RuleFor(x => x.PageSize)
    .InclusiveBetween(1, 100)
    .WithMessage("Page size must be between 1 and 100.");
```

### 3. Database Indexes

Ensure the following indexes exist (created in Phase 2 migrations):

```sql
CREATE INDEX idx_users_tenant_id ON users(tenant_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_is_deleted ON users(is_deleted);
CREATE INDEX idx_users_created_at ON users(created_at_utc);
```

These indexes optimize:

- Tenant isolation filtering (`tenant_id`)
- Email uniqueness checks (`email`)
- Soft delete filtering (`is_deleted`)
- Default ordering (`created_at_utc`)

### 4. Caching Strategy (Future Phase)

Phase 16 will add Redis caching for frequently accessed users:

```csharp
// Future implementation
var cachedUser = await _cache.GetAsync<User>($"user:{userId}");
if (cachedUser != null)
    return cachedUser;

var user = await _userRepository.GetByIdAsync(userId);
await _cache.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(30));
```

---

## Security Considerations

### 1. Tenant Isolation

All commands and queries verify the user belongs to the current tenant context:

```csharp
if (user.TenantId != _tenantContext.TenantId)
{
    return Result.Failure<UserDetailDto>(Error.NotFound("User not found."));
}
```

This prevents cross-tenant data access even if a user somehow obtains another tenant's user ID.

### 2. Password Strength Enforcement

The `PasswordValidator` enforces strong passwords during user creation:

- Minimum 8 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (!@#$%^&\*()...)

### 3. Soft Delete for Audit Trails

Deleted users remain in the database with `IsDeleted = true`, preserving:

- Historical data integrity
- Foreign key relationships
- Audit logs and compliance records

### 4. Token Revocation on Delete

When a user is deleted, all their refresh tokens are revoked:

```csharp
await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, cancellationToken);
```

This immediately invalidates all active sessions across all devices.

---

## Known Limitations & Future Enhancements

### Current Limitations

1. **No Email Verification**: Users are created active without email confirmation (future enhancement).
2. **No Password Reset via Users API**: Password changes require the user to be authenticated (use Auth feature).
3. **Role Assignment Lacks Validation**: No check for maximum roles per user or role compatibility rules.
4. **No User Import/Export**: Bulk operations not yet implemented.

### Planned Enhancements (Future Phases)

- **Phase 11 (Roles & Permissions)**: Add permission-based authorization checks before role assignment.
- **Phase 16 (Redis Caching)**: Cache frequently accessed users to reduce database load.
- **Phase 17 (Unit Tests)**: Comprehensive test coverage for all handlers.
- **Phase 18 (Integration Tests)**: End-to-end API testing with real database.
- **Phase 19 (Documentation)**: OpenAPI/Swagger documentation with request/response examples.

---

## Migration Notes

No new database migrations required — Phase 8 reuses the `users` and `roles` tables created in Phase 2 and Phase 4.

However, verify the following indexes exist:

```sql
-- Check existing indexes
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'users';

-- Expected indexes:
-- idx_users_tenant_id
-- idx_users_email
-- idx_users_is_deleted
-- idx_users_created_at
```

---

## Configuration

No additional configuration required beyond existing settings. The user feature uses:

- **JWT settings** from Phase 6 (for `CurrentUserService`)
- **Multi-tenancy settings** from Phase 5 (for `TenantContext`)
- **Database connection** from Phase 4 (for `UserRepository`)

---

## Checklist

Use this checklist to track Phase 8 completion:

### Day 1: Create & Update User Commands

- [ ] Create `CreateUserCommand.cs`, validator, and handler
- [ ] Create `UpdateUserCommand.cs`, validator, and handler
- [ ] Test create and update commands

### Day 2: Delete User & Role Assignment Commands

- [ ] Create `DeleteUserCommand.cs` and handler (soft delete)
- [ ] Create `AssignRoleCommand.cs`, validator, and handler
- [ ] Create `RemoveRoleCommand.cs` and handler
- [ ] Test role assignment

### Day 3: User Query DTOs & Mappings

- [ ] Create `UserDto.cs` and `UserDetailDto.cs`
- [ ] Create `UserRoleDto.cs`
- [ ] Configure AutoMapper mappings (`UserMappingProfile.cs`)
- [ ] Test mapping from entities to DTOs

### Day 4: User Queries

- [ ] Create `GetUsersQuery.cs`, validator, and handler
- [ ] Create `GetUserByIdQuery.cs`, validator, and handler
- [ ] Create `GetUserRolesQuery.cs`, validator, and handler
- [ ] Test pagination, filtering, and sorting

### Day 5: Testing & Documentation

- [ ] Write unit tests for `CreateUserCommandHandler`
- [ ] Write unit tests for `UpdateUserCommandHandler`
- [ ] Write unit tests for `DeleteUserCommandHandler`
- [ ] Write unit tests for `AssignRoleCommandHandler`
- [ ] Write unit tests for `GetUsersQueryHandler` (pagination, search, filtering, tenant isolation)
- [ ] Write unit tests for `GetUserByIdQueryHandler`
- [ ] Document users feature in code comments
- [ ] Add example requests/responses in documentation (this file)

---

## Key Takeaways

1. **CQRS separates concerns**: Commands mutate state using domain entities; queries read data using DTOs.
2. **Tenant isolation is automatic**: `ITenantContext` ensures users can never access data from other tenants.
3. **Soft delete preserves history**: Deleted users remain in the database for auditability.
4. **Idempotent operations simplify clients**: Role assignment/removal can be safely retried.
5. **Pagination prevents performance issues**: Maximum page size of 100 protects the API from large result sets.
6. **AutoMapper decouples layers**: Domain entities can evolve without breaking API contracts.

---

**End of Phase 8 Documentation**
