# API Response Models

This document details the standardized response models used across all API endpoints in Phase 9.

---

## Overview

All HTTP responses follow a consistent structure to:

- Provide predictable client-side parsing
- Include metadata (success status, timestamp)
- Handle errors uniformly
- Support pagination for list endpoints

---

## Core Response Models

### 1. ApiResponse&lt;T&gt;

Generic wrapper for all API responses (success and error cases).

**File:** `Application/Common/Models/ApiResponse.cs`

```csharp
namespace Application.Common.Models;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public sealed record ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation succeeded
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The response payload (null on failure)
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Human-readable message (success or error description)
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Validation errors dictionary (key: field name, value: error messages)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }

    /// <summary>
    /// UTC timestamp when response was generated
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
        => new()
        {
            Success = true,
            Data = data,
            Message = message
        };

    /// <summary>
    /// Creates a failure response
    /// </summary>
    public static ApiResponse<T> Fail(string message, Dictionary<string, string[]>? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}
```

**Usage in Controllers:**

```csharp
// Success response
return Ok(ApiResponse<UserDto>.Ok(userDto, "User created successfully"));

// Error response
return BadRequest(ApiResponse<object>.Fail("Invalid email format"));

// Error with validation details
return UnprocessableEntity(ApiResponse<object>.Fail(
    "Validation failed",
    new Dictionary<string, string[]>
    {
        { "Email", new[] { "Email is required", "Email must be valid" } },
        { "Password", new[] { "Password must be at least 8 characters" } }
    }
));
```

---

### 2. PaginatedResponse&lt;T&gt;

Response model for paginated list endpoints.

**File:** `Application/Common/Models/PaginatedResponse.cs`

```csharp
namespace Application.Common.Models;

/// <summary>
/// Paginated list response with metadata
/// </summary>
/// <typeparam name="T">Type of items in the list</typeparam>
public sealed record PaginatedResponse<T>
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public List<T> Items { get; init; } = new();

    /// <summary>
    /// Current page number (1-indexed)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a paginated response from a query result
    /// </summary>
    public static PaginatedResponse<T> Create(
        List<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
        => new()
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
}
```

**Usage in Query Handlers:**

```csharp
public async Task<Result<PaginatedResponse<UserDto>>> Handle(
    GetUsersQuery request,
    CancellationToken cancellationToken)
{
    var users = await _userRepository.GetPaginatedAsync(
        request.PageNumber,
        request.PageSize,
        request.SearchTerm);

    var userDtos = _mapper.Map<List<UserDto>>(users.Items);

    var response = PaginatedResponse<UserDto>.Create(
        userDtos,
        request.PageNumber,
        request.PageSize,
        users.TotalCount);

    return Result<PaginatedResponse<UserDto>>.Success(response);
}
```

---

## Response Scenarios

### Success Responses

#### Single Entity (200 OK)

**Request:**

```http
GET /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "fullName": "John Doe",
    "isActive": true,
    "createdAt": "2026-05-01T10:00:00Z"
  },
  "message": null,
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Paginated List (200 OK)

**Request:**

```http
GET /api/v1/users?pageNumber=1&pageSize=2
```

**Response:**

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
    "pageSize": 2,
    "totalCount": 25,
    "totalPages": 13,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": null,
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Creation Success (200 OK)

**Request:**

```http
POST /api/v1/users
{
  "email": "newuser@example.com",
  "password": "SecurePass123!",
  "fullName": "Alice Johnson"
}
```

**Response:**

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
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Deletion Success (200 OK)

**Request:**

```http
DELETE /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response:**

```json
{
  "success": true,
  "data": null,
  "message": "User deleted successfully",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

---

### Error Responses

#### Validation Error (422 Unprocessable Entity)

**Request:**

```http
POST /api/v1/auth/register
{
  "email": "invalid-email",
  "password": "123"
}
```

**Response:**

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "Email": ["Email must be a valid email address"],
    "Password": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter",
      "Password must contain at least one digit"
    ],
    "FullName": ["Full name is required"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Not Found (404 Not Found)

**Request:**

```http
GET /api/v1/users/00000000-0000-0000-0000-000000000000
```

**Response:**

```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Unauthorized (401 Unauthorized)

**Request:**

```http
POST /api/v1/auth/login
{
  "email": "user@example.com",
  "password": "WrongPassword123!",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:**

```json
{
  "success": false,
  "data": null,
  "message": "Invalid email or password",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Forbidden (403 Forbidden)

**Request:**

```http
POST /api/v1/users
Authorization: Bearer <token-without-admin-role>
```

**Response:**

```json
{
  "success": false,
  "data": null,
  "message": "Access denied. Insufficient permissions.",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Conflict (409 Conflict)

**Request:**

```http
POST /api/v1/auth/register
{
  "email": "existing@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe"
}
```

**Response:**

```json
{
  "success": false,
  "data": null,
  "message": "User with this email already exists",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Bad Request (400 Bad Request)

**Request:**

```http
PUT /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
{
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "fullName": "Updated Name"
}
```

**Response:**

```json
{
  "success": false,
  "data": null,
  "message": "Route ID does not match command ID",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

#### Internal Server Error (500 Internal Server Error)

**Response:**

```json
{
  "success": false,
  "data": null,
  "message": "An unexpected error occurred. Please try again later.",
  "errors": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

---

## HTTP Status Code Mapping

| Domain Error Code | HTTP Status               | Use Case                                        |
| ----------------- | ------------------------- | ----------------------------------------------- |
| Success           | 200 OK                    | Successful operation                            |
| NotFound          | 404 Not Found             | Resource doesn't exist                          |
| Unauthorized      | 401 Unauthorized          | Invalid credentials or expired token            |
| Forbidden         | 403 Forbidden             | Authenticated but lacks permission              |
| Conflict          | 409 Conflict              | Resource already exists (e.g., duplicate email) |
| ValidationError   | 422 Unprocessable Entity  | Input validation failed                         |
| BadRequest        | 400 Bad Request           | Malformed request or business rule violation    |
| InternalError     | 500 Internal Server Error | Unhandled exception                             |

**Implementation in ApiController:**

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
        "Unauthorized" => Unauthorized(ApiResponse<T>.Fail(result.Error.Message)),
        "Forbidden" => StatusCode(403, ApiResponse<T>.Fail(result.Error.Message)),
        "Conflict" => Conflict(ApiResponse<T>.Fail(result.Error.Message)),
        "ValidationError" => UnprocessableEntity(ApiResponse<T>.Fail(result.Error.Message)),
        _ => BadRequest(ApiResponse<T>.Fail(result.Error.Message))
    };
}
```

---

## Client-Side Consumption

### TypeScript Interface

```typescript
interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: Record<string, string[]> | null;
  timestamp: string;
}

interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
```

### JavaScript Fetch Example

```javascript
async function loginUser(email, password, tenantId) {
  try {
    const response = await fetch("http://localhost:5000/api/v1/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password, tenantId }),
    });

    const apiResponse = await response.json();

    if (apiResponse.success) {
      const { accessToken, refreshToken } = apiResponse.data;
      localStorage.setItem("accessToken", accessToken);
      localStorage.setItem("refreshToken", refreshToken);
      return apiResponse.data;
    } else {
      throw new Error(apiResponse.message || "Login failed");
    }
  } catch (error) {
    console.error("Login error:", error);
    throw error;
  }
}
```

### C# Client Example

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", request);

        var apiResponse = await response.Content
            .ReadFromJsonAsync<ApiResponse<LoginResponse>>();

        if (!apiResponse.Success)
        {
            throw new ApiException(apiResponse.Message);
        }

        return apiResponse;
    }
}
```

---

## Best Practices

### 1. Always Use ApiResponse Wrapper

**❌ Don't:**

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Get(Guid id)
{
    var user = await _userRepository.GetByIdAsync(id);
    return Ok(user); // Raw entity, no metadata
}
```

**✅ Do:**

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Get(Guid id)
{
    var result = await Sender.Send(new GetUserByIdQuery(id));
    return FromResult(result); // Wrapped in ApiResponse
}
```

### 2. Include Meaningful Messages

**❌ Don't:**

```csharp
return BadRequest(ApiResponse<object>.Fail("Error"));
```

**✅ Do:**

```csharp
return BadRequest(ApiResponse<object>.Fail("Email format is invalid. Please provide a valid email address."));
```

### 3. Use Proper HTTP Status Codes

**❌ Don't:**

```csharp
return Ok(ApiResponse<object>.Fail("User not found")); // 200 with success=false
```

**✅ Do:**

```csharp
return NotFound(ApiResponse<object>.Fail("User not found")); // 404
```

### 4. Provide Validation Details

**❌ Don't:**

```csharp
return BadRequest(ApiResponse<object>.Fail("Invalid input"));
```

**✅ Do:**

```csharp
return UnprocessableEntity(ApiResponse<object>.Fail(
    "Validation failed",
    new Dictionary<string, string[]>
    {
        { "Email", new[] { "Email is required" } },
        { "Password", new[] { "Password must be at least 8 characters" } }
    }
));
```

---

## Testing Response Models

### Unit Test Example

```csharp
[Fact]
public void ApiResponse_Ok_ShouldSetSuccessTrue()
{
    // Arrange
    var userData = new UserDto(
        Id: Guid.NewGuid(),
        Email: "test@example.com",
        FullName: "Test User"
    );

    // Act
    var response = ApiResponse<UserDto>.Ok(userData, "User retrieved");

    // Assert
    Assert.True(response.Success);
    Assert.Equal(userData, response.Data);
    Assert.Equal("User retrieved", response.Message);
    Assert.Null(response.Errors);
}

[Fact]
public void ApiResponse_Fail_ShouldSetSuccessFalse()
{
    // Arrange
    var errors = new Dictionary<string, string[]>
    {
        { "Email", new[] { "Email is required" } }
    };

    // Act
    var response = ApiResponse<UserDto>.Fail("Validation failed", errors);

    // Assert
    Assert.False(response.Success);
    Assert.Null(response.Data);
    Assert.Equal("Validation failed", response.Message);
    Assert.Equal(errors, response.Errors);
}
```

---

## Summary

- **ApiResponse<T>**: Universal response wrapper for success/error cases
- **PaginatedResponse<T>**: Specialized wrapper for paginated lists
- **Consistent Structure**: All endpoints return the same JSON format
- **HTTP Status Codes**: Proper mapping from domain errors to HTTP semantics
- **Client-Friendly**: Predictable structure enables easy client-side parsing
- **Error Details**: Validation errors include field-level details

This standardization ensures a clean, predictable API contract for all consumers.
