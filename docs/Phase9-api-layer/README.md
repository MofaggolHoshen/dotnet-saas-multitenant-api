# Phase 9 — API Layer & Controllers

**Phase**: 9  
**Status**: ⚪ Not Started  
**Depends On**: Phase 8 (Users Feature - CQRS Implementation)  
**Duration**: 4 days

---

## Overview

Phase 9 exposes the authentication and user management features built in Phases 7 and 8 through RESTful HTTP endpoints. This phase implements a clean API layer using ASP.NET Core controllers that delegate all business logic to MediatR handlers, maintaining strict separation of concerns.

The API layer provides:

- **Standardized Response Format**: Consistent JSON structure across all endpoints
- **HTTP Status Code Mapping**: Proper translation from domain results to HTTP semantics
- **Route Versioning**: API versioning strategy for backward compatibility
- **Authentication & Authorization**: JWT-based security with role-based access control
- **CORS Configuration**: Cross-origin support for SPA and mobile clients

This phase completes the presentation layer, enabling external clients to interact with the application's authentication and user management capabilities.

---

## Files Added / Modified

### Controllers

| Action   | File                                              | Description                                          |
| -------- | ------------------------------------------------- | ---------------------------------------------------- |
| ➕ Added | `Presentation/API/Controllers/ApiController.cs`   | Abstract base controller with standardized responses |
| ➕ Added | `Presentation/API/Controllers/AuthController.cs`  | Endpoints for login, register, and token refresh     |
| ➕ Added | `Presentation/API/Controllers/UsersController.cs` | CRUD endpoints for user management                   |

### Configuration

| Action      | File                                | Description                           |
| ----------- | ----------------------------------- | ------------------------------------- |
| 🔧 Modified | `Presentation/API/Program.cs`       | Configure API services and middleware |
| ➕ Added    | `Presentation/API/appsettings.json` | API configuration settings            |

### Response Models

| Action   | File                                                   | Description                     |
| -------- | ------------------------------------------------------ | ------------------------------- |
| ➕ Added | `Application/Common/Models/ApiResponse.cs`             | Generic API response wrapper    |
| ➕ Added | `Application/Common/Models/PaginatedResponse.cs`       | Paginated list response wrapper |
| ➕ Added | `Application/Common/Models/ValidationErrorResponse.cs` | Validation error details        |

---

## Architecture Decisions

### 1. Thin Controllers Pattern

Controllers are kept minimal and serve only as HTTP adapters. All business logic resides in MediatR handlers.

**Why This Matters:**

- Controllers become trivial to test (no business logic)
- Business logic is reusable across different presentation layers (gRPC, SignalR, etc.)
- Changes to HTTP concerns don't affect business rules

**Example:**

```csharp
// ❌ Fat Controller (Business Logic in Controller)
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Validation
    if (string.IsNullOrEmpty(request.Email))
        return BadRequest("Email is required");

    // Business Logic
    var user = await _userRepository.GetByEmailAsync(request.Email);
    if (user == null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        return Unauthorized("Invalid credentials");

    // Token Generation
    var token = _jwtService.GenerateToken(user);
    return Ok(new { token });
}

// ✅ Thin Controller (Delegate to MediatR)
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginCommand command)
    => FromResult(await Sender.Send(command));
```

### 2. Result Pattern to HTTP Mapping

Domain `Result<T>` objects are translated to appropriate HTTP status codes in the base controller.

**Mapping Strategy:**

| Domain Result     | HTTP Status              | Response Body             |
| ----------------- | ------------------------ | ------------------------- |
| Success           | 200 OK                   | `ApiResponse<T>.Ok(T)`    |
| Error (NotFound)  | 404 Not Found            | `ApiResponse<T>.Fail()`   |
| Error (Forbidden) | 403 Forbidden            | Empty                     |
| Error (Other)     | 400 Bad Request          | `ApiResponse<T>.Fail()`   |
| Validation Error  | 422 Unprocessable Entity | `ValidationErrorResponse` |

**Implementation:**

```csharp
protected IActionResult FromResult<T>(Result<T> result)
{
    if (result.IsSuccess)
    {
        return Ok(ApiResponse<T>.Ok(result.Value));
    }

    return result.Error.Code switch
    {
        "NotFound" => NotFound(ApiResponse<T>.Fail(result.Error.Message)),
        "Forbidden" => Forbid(),
        "Unauthorized" => Unauthorized(ApiResponse<T>.Fail(result.Error.Message)),
        "Conflict" => Conflict(ApiResponse<T>.Fail(result.Error.Message)),
        _ => BadRequest(ApiResponse<T>.Fail(result.Error.Message))
    };
}
```

### 3. API Response Standardization

All API responses follow a consistent structure for easier client-side consumption.

**Success Response:**

```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "fullName": "John Doe"
  },
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Error Response:**

```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Validation Error Response:**

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "Email": ["Email is required", "Email must be valid"],
    "Password": ["Password must be at least 8 characters"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

### 4. Route Versioning Strategy

API versioning through URL path provides explicit, visible version control.

**Route Pattern:**

```
/api/v1/[controller]/[action]
```

**Examples:**

```
POST /api/v1/auth/login
POST /api/v1/auth/register
GET  /api/v1/users
GET  /api/v1/users/{id}
POST /api/v1/users
PUT  /api/v1/users/{id}
DELETE /api/v1/users/{id}
```

**Benefits:**

- Clear version communication to clients
- Easy to deprecate old versions
- No header-based version negotiation complexity
- Supports parallel versions during migration

---

## Implementation Details

### Base API Controller

The `ApiController` provides foundation services for all endpoints:

**File:** `Presentation/API/Controllers/ApiController.cs`

```csharp
using Application.Common.Models;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/v1/[controller]")]
public abstract class ApiController : ControllerBase
{
    private ISender? _sender;
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Converts a domain Result to an appropriate HTTP response
    /// </summary>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Value));
        }

        return result.Error.Code switch
        {
            "NotFound" => NotFound(ApiResponse<T>.Fail(result.Error.Message)),
            "Forbidden" => Forbid(),
            "Unauthorized" => Unauthorized(ApiResponse<T>.Fail(result.Error.Message)),
            "Conflict" => Conflict(ApiResponse<T>.Fail(result.Error.Message)),
            _ => BadRequest(ApiResponse<T>.Fail(result.Error.Message))
        };
    }

    /// <summary>
    /// Converts a domain Result (no value) to an appropriate HTTP response
    /// </summary>
    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<object>.Ok(null));
        }

        return result.Error.Code switch
        {
            "NotFound" => NotFound(ApiResponse<object>.Fail(result.Error.Message)),
            "Forbidden" => Forbid(),
            "Unauthorized" => Unauthorized(ApiResponse<object>.Fail(result.Error.Message)),
            "Conflict" => Conflict(ApiResponse<object>.Fail(result.Error.Message)),
            _ => BadRequest(ApiResponse<object>.Fail(result.Error.Message))
        };
    }
}
```

**Key Features:**

- **Lazy Sender Injection**: MediatR sender is resolved on-demand from `HttpContext`
- **Generic Result Mapping**: Works with any `Result<T>` type
- **Consistent Responses**: All endpoints return standardized `ApiResponse<T>`
- **Error Code Translation**: Domain error codes map to HTTP status codes

---

### Authentication Controller

**File:** `Presentation/API/Controllers/AuthController.cs`

```csharp
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Controllers;

/// <summary>
/// Handles authentication operations: login, registration, token refresh
/// </summary>
public sealed class AuthController : ApiController
{
    /// <summary>
    /// Authenticates a user and returns JWT tokens
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>Access token and refresh token</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
        => FromResult(await Sender.Send(command));

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>Newly created user information</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        => FromResult(await Sender.Send(command));

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token
    /// </summary>
    /// <param name="command">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        => FromResult(await Sender.Send(command));
}
```

**Endpoints:**

| Method | Route                 | Auth      | Description                      |
| ------ | --------------------- | --------- | -------------------------------- |
| POST   | /api/v1/auth/login    | Anonymous | Authenticate user, return tokens |
| POST   | /api/v1/auth/register | Anonymous | Create new user account          |
| POST   | /api/v1/auth/refresh  | Anonymous | Refresh expired access token     |

**Request/Response Examples:**

<details>
<summary><b>POST /api/v1/auth/login</b></summary>

**Request:**

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGVzdC1yZWZyZXNoLXRva2Vu...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Response (401 Unauthorized):**

```json
{
  "success": false,
  "data": null,
  "message": "Invalid email or password",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

</details>

<details>
<summary><b>POST /api/v1/auth/register</b></summary>

**Request:**

```json
{
  "email": "newuser@example.com",
  "password": "SecurePassword123!",
  "fullName": "John Doe",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "email": "newuser@example.com",
    "fullName": "John Doe"
  },
  "message": "User registered successfully",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Response (409 Conflict):**

```json
{
  "success": false,
  "data": null,
  "message": "User with this email already exists",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

</details>

---

### Users Controller

**File:** `Presentation/API/Controllers/UsersController.cs`

```csharp
using Application.Features.Users.Commands.AssignRole;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.DeleteUser;
using Application.Features.Users.Commands.RemoveRole;
using Application.Features.Users.Commands.UpdateUser;
using Application.Features.Users.Queries.GetUserById;
using Application.Features.Users.Queries.GetUserRoles;
using Application.Features.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Controllers;

/// <summary>
/// Handles user management operations: CRUD, role assignment
/// </summary>
[Authorize]
public sealed class UsersController : ApiController
{
    /// <summary>
    /// Retrieves a paginated list of users in the current tenant
    /// </summary>
    /// <param name="query">Pagination and filter parameters</param>
    /// <returns>Paginated user list</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        => FromResult(await Sender.Send(query));

    /// <summary>
    /// Retrieves detailed information for a specific user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
        => FromResult(await Sender.Send(new GetUserByIdQuery(id)));

    /// <summary>
    /// Retrieves all roles assigned to a specific user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>List of roles</returns>
    [HttpGet("{id:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse<List<UserRoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoles(Guid id)
        => FromResult(await Sender.Send(new GetUserRolesQuery(id)));

    /// <summary>
    /// Creates a new user in the current tenant (Admin only)
    /// </summary>
    /// <param name="command">User creation details</param>
    /// <returns>Created user information</returns>
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        => FromResult(await Sender.Send(command));

    /// <summary>
    /// Updates an existing user's information (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">Updated user details</param>
    /// <returns>Updated user information</returns>
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command)
        => id != command.UserId
            ? BadRequest(ApiResponse<object>.Fail("Route ID does not match command ID"))
            : FromResult(await Sender.Send(command));

    /// <summary>
    /// Soft deletes a user (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Deletion confirmation</returns>
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
        => FromResult(await Sender.Send(new DeleteUserCommand(id)));

    /// <summary>
    /// Assigns a role to a user (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">Role assignment details</param>
    /// <returns>Assignment confirmation</returns>
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleCommand command)
        => id != command.UserId
            ? BadRequest(ApiResponse<object>.Fail("Route ID does not match command ID"))
            : FromResult(await Sender.Send(command));

    /// <summary>
    /// Removes a role from a user (Admin only)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">Role removal details</param>
    /// <returns>Removal confirmation</returns>
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
        => FromResult(await Sender.Send(new RemoveRoleCommand(id, roleId)));
}
```

**Endpoints:**

| Method | Route                             | Auth          | Description               |
| ------ | --------------------------------- | ------------- | ------------------------- |
| GET    | /api/v1/users                     | Authenticated | Get paginated user list   |
| GET    | /api/v1/users/{id}                | Authenticated | Get user details          |
| GET    | /api/v1/users/{id}/roles          | Authenticated | Get user's roles          |
| POST   | /api/v1/users                     | Admin only    | Create new user           |
| PUT    | /api/v1/users/{id}                | Admin only    | Update user               |
| DELETE | /api/v1/users/{id}                | Admin only    | Delete user (soft delete) |
| POST   | /api/v1/users/{id}/roles          | Admin only    | Assign role to user       |
| DELETE | /api/v1/users/{id}/roles/{roleId} | Admin only    | Remove role from user     |

**Request/Response Examples:**

<details>
<summary><b>GET /api/v1/users?pageNumber=1&pageSize=10</b></summary>

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "email": "user1@example.com",
        "fullName": "John Doe",
        "isActive": true,
        "createdAt": "2026-05-01T10:00:00Z"
      },
      {
        "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
        "email": "user2@example.com",
        "fullName": "Jane Smith",
        "isActive": true,
        "createdAt": "2026-05-02T11:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 2,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

</details>

<details>
<summary><b>POST /api/v1/users</b></summary>

**Request:**

```json
{
  "email": "newuser@example.com",
  "password": "SecurePassword123!",
  "fullName": "Alice Johnson",
  "initialRoles": ["User"]
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "email": "newuser@example.com",
    "fullName": "Alice Johnson",
    "isActive": true,
    "createdAt": "2026-05-15T19:24:15.994Z"
  },
  "message": "User created successfully",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

</details>

---

## Program.cs Configuration

**File:** `Presentation/API/Program.cs`

```csharp
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== Add Services to Container =====

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application & Infrastructure Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== Configure Middleware Pipeline =====

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Security
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Tenant Context Middleware (from Phase 5)
app.UseTenantContext();

// Route Controllers
app.MapControllers();

app.Run();
```

**Configuration Highlights:**

1. **JSON Serialization**:
   - Camel case for property names (`userId` instead of `UserId`)
   - Null value suppression (cleaner responses)
   - Reference cycle handling (prevents infinite loops)

2. **JWT Authentication**:
   - Validates issuer, audience, lifetime, and signature
   - Zero clock skew for precise expiration
   - Configured from `appsettings.json`

3. **CORS Policy**:
   - Development: Allow all origins
   - Production: Restrict to specific domains

4. **Middleware Order** (Critical):
   ```
   UseHttpsRedirection
   → UseCors
   → UseAuthentication
   → UseAuthorization
   → UseTenantContext (custom)
   → MapControllers
   ```

---

## Response Models

### ApiResponse<T>

**File:** `Application/Common/Models/ApiResponse.cs`

```csharp
namespace Application.Common.Models;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new()
        {
            Success = true,
            Data = data,
            Message = message
        };

    public static ApiResponse<T> Fail(string message, Dictionary<string, string[]>? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}
```

### PaginatedResponse<T>

**File:** `Application/Common/Models/PaginatedResponse.cs`

```csharp
namespace Application.Common.Models;

public sealed record PaginatedResponse<T>
{
    public List<T> Items { get; init; } = new();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

---

## Testing Strategy

### 1. Controller Unit Tests

Test HTTP response mapping without hitting real handlers.

**File:** `Tests/UnitTests/Presentation/Controllers/AuthControllerTests.cs`

```csharp
[Fact]
public async Task Login_WithValidCredentials_Returns200WithTokens()
{
    // Arrange
    var mockSender = new Mock<ISender>();
    mockSender
        .Setup(s => s.Send(It.IsAny<LoginCommand>(), default))
        .ReturnsAsync(Result<LoginResponse>.Success(new LoginResponse(
            AccessToken: "test-token",
            RefreshToken: "test-refresh",
            ExpiresIn: 3600,
            TokenType: "Bearer"
        )));

    var controller = new AuthController { Sender = mockSender.Object };

    // Act
    var result = await controller.Login(new LoginCommand(
        Email: "test@example.com",
        Password: "password",
        TenantId: Guid.NewGuid()
    ));

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = Assert.IsType<ApiResponse<LoginResponse>>(okResult.Value);
    Assert.True(response.Success);
    Assert.Equal("test-token", response.Data.AccessToken);
}
```

### 2. Integration Tests

Test full request → database flow with in-memory database.

**File:** `Tests/IntegrationTests/API/AuthEndpointsTests.cs`

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsJwtToken()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new
    {
        email = "admin@tenant1.com",
        password = "Admin123!",
        tenantId = _tenant1Id
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    result.Success.Should().BeTrue();
    result.Data.AccessToken.Should().NotBeNullOrEmpty();
}
```

### 3. Manual Testing with curl

**Login:**

```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@tenant1.com",
    "password": "Admin123!",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

**Get Users (with token):**

```bash
curl -X GET http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Validation Flow

Validation happens before reaching controllers via FluentValidation filter.

**Request Flow:**

```
HTTP Request
  ↓
Model Binding
  ↓
FluentValidation (Automatic)
  ├─ Valid → Controller Action
  └─ Invalid → 422 ValidationErrorResponse
```

**Example Validation Error:**

**Request:**

```json
{
  "email": "",
  "password": "123"
}
```

**Response (422 Unprocessable Entity):**

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

---

## Security Considerations

### 1. Authorization Rules

| Endpoint               | Required Role          | Reasoning                 |
| ---------------------- | ---------------------- | ------------------------- |
| GET /users             | Any authenticated      | Users can view colleagues |
| POST /users            | TenantAdmin/SuperAdmin | Only admins create users  |
| PUT /users/{id}        | TenantAdmin/SuperAdmin | Only admins modify users  |
| DELETE /users/{id}     | TenantAdmin/SuperAdmin | Only admins delete users  |
| POST /users/{id}/roles | TenantAdmin/SuperAdmin | Only admins assign roles  |

### 2. Tenant Isolation

All user operations are automatically scoped to the current tenant via:

- JWT claim `tenant_id` extracted by `TenantContextMiddleware`
- Repository queries filtered by `TenantId`
- No cross-tenant data leakage possible

### 3. Input Validation

- Email format validation
- Password strength requirements (min 8 chars, uppercase, lowercase, digit)
- ID format validation (GUID)
- Pagination limits (max 100 items per page)

---

## Common Patterns

### Pattern 1: Route ID Validation

Ensure route parameter matches command property to prevent ID manipulation.

```csharp
[HttpPut("{id:guid}")]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command)
    => id != command.UserId
        ? BadRequest(ApiResponse<object>.Fail("Route ID does not match command ID"))
        : FromResult(await Sender.Send(command));
```

### Pattern 2: Anonymous Endpoints

Use `[AllowAnonymous]` to bypass authentication for public endpoints.

```csharp
[AllowAnonymous]
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginCommand command)
    => FromResult(await Sender.Send(command));
```

### Pattern 3: Role-Based Authorization

Restrict endpoints to specific roles.

```csharp
[Authorize(Roles = "TenantAdmin,SuperAdmin")]
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    => FromResult(await Sender.Send(command));
```

---

## Next Steps

After completing Phase 9:

1. **Phase 10**: Implement Tenant Management (CRUD for tenants)
2. **Phase 11**: Implement Role Management (dynamic role assignment)
3. **Phase 12**: Add MediatR Pipeline Behaviors (logging, validation, caching)
4. **Phase 13**: Add Global Exception Middleware
5. **Phase 14**: Configure Swagger/OpenAPI Documentation

---

## Checklist

- [ ] Create `ApiController` base class
- [ ] Create `ApiResponse<T>` and `PaginatedResponse<T>` models
- [ ] Implement `AuthController` with login, register, refresh endpoints
- [ ] Implement `UsersController` with CRUD endpoints
- [ ] Configure Program.cs (JWT, CORS, JSON serialization)
- [ ] Test auth endpoints (Postman/curl)
- [ ] Test user endpoints with authorization
- [ ] Write controller unit tests
- [ ] Write API integration tests
- [ ] Verify tenant isolation in multi-tenant scenarios

---

## References

- [ASP.NET Core Controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [JWT Authentication](https://jwt.io/)
- [Result Pattern in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
- [API Versioning Best Practices](https://restfulapi.net/versioning/)
