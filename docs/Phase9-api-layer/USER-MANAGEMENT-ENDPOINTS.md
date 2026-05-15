# User Management Endpoints

This document provides detailed specifications for all user management API endpoints in Phase 9.

---

## Overview

The `UsersController` exposes endpoints for complete user CRUD operations and role management. All endpoints require authentication, with administrative operations restricted to users with `TenantAdmin` or `SuperAdmin` roles.

**Features:**

- List users with pagination and filtering
- View detailed user information
- Create new users (Admin only)
- Update existing users (Admin only)
- Soft delete users (Admin only)
- Manage user roles (Admin only)

---

## Base Route

```
/api/v1/users
```

---

## Endpoints

### 1. Get Users (Paginated List)

Retrieves a paginated list of users in the current tenant with optional filtering.

**Endpoint:**

```
GET /api/v1/users
```

**Authorization:** Authenticated (any role)

**Query Parameters:**

| Parameter      | Type   | Required | Default     | Description                             |
| -------------- | ------ | -------- | ----------- | --------------------------------------- |
| pageNumber     | int    | No       | 1           | Page number (1-indexed)                 |
| pageSize       | int    | No       | 10          | Items per page (max 100)                |
| searchTerm     | string | No       | null        | Search in email, full name              |
| isActive       | bool   | No       | null        | Filter by active status                 |
| orderBy        | string | No       | "CreatedAt" | Sort field (CreatedAt, Email, FullName) |
| orderDirection | string | No       | "Desc"      | Sort direction (Asc, Desc)              |

**Example Request:**

```
GET /api/v1/users?pageNumber=1&pageSize=20&searchTerm=john&isActive=true&orderBy=Email&orderDirection=Asc
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "email": "john.doe@example.com",
        "fullName": "John Doe",
        "isActive": true,
        "createdAt": "2026-05-01T10:00:00Z",
        "lastLoginAt": "2026-05-15T19:20:00Z"
      },
      {
        "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
        "email": "john.smith@example.com",
        "fullName": "John Smith",
        "isActive": true,
        "createdAt": "2026-05-02T11:00:00Z",
        "lastLoginAt": "2026-05-14T08:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 2,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**UserDto Schema:**

| Field       | Type     | Description                        |
| ----------- | -------- | ---------------------------------- |
| id          | Guid     | User's unique identifier           |
| email       | string   | User's email address               |
| fullName    | string   | User's full name                   |
| isActive    | bool     | Whether the user account is active |
| createdAt   | DateTime | When the user was created          |
| lastLoginAt | DateTime | Last login timestamp (nullable)    |

**Error Responses:**

**401 Unauthorized** - Missing or invalid token:

```json
{
  "success": false,
  "data": null,
  "message": "Unauthorized",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**422 Unprocessable Entity** - Invalid pagination:

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "PageNumber": ["Page number must be at least 1"],
    "PageSize": ["Page size must be between 1 and 100"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**cURL Example:**

```bash
curl -X GET "http://localhost:5000/api/v1/users?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 2. Get User By ID

Retrieves detailed information for a specific user.

**Endpoint:**

```
GET /api/v1/users/{id}
```

**Authorization:** Authenticated (any role)

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| id        | Guid | Yes      | User ID     |

**Example Request:**

```
GET /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "john.doe@example.com",
    "fullName": "John Doe",
    "isActive": true,
    "tenantId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "tenantName": "Acme Corporation",
    "roles": [
      {
        "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
        "name": "TenantAdmin"
      },
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "User"
      }
    ],
    "createdAt": "2026-05-01T10:00:00Z",
    "updatedAt": "2026-05-10T15:30:00Z",
    "lastLoginAt": "2026-05-15T19:20:00Z"
  },
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**UserDetailDto Schema:**

| Field       | Type          | Description                        |
| ----------- | ------------- | ---------------------------------- |
| id          | Guid          | User's unique identifier           |
| email       | string        | User's email address               |
| fullName    | string        | User's full name                   |
| isActive    | bool          | Whether the user account is active |
| tenantId    | Guid          | Tenant the user belongs to         |
| tenantName  | string        | Name of the tenant                 |
| roles       | UserRoleDto[] | List of roles assigned to user     |
| createdAt   | DateTime      | When the user was created          |
| updatedAt   | DateTime      | Last update timestamp (nullable)   |
| lastLoginAt | DateTime      | Last login timestamp (nullable)    |

**Error Responses:**

**404 Not Found** - User doesn't exist:

```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**cURL Example:**

```bash
curl -X GET http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 3. Get User Roles

Retrieves all roles assigned to a specific user.

**Endpoint:**

```
GET /api/v1/users/{id}/roles
```

**Authorization:** Authenticated (any role)

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| id        | Guid | Yes      | User ID     |

**Example Request:**

```
GET /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/roles
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": [
    {
      "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "name": "TenantAdmin",
      "description": "Administrator of the tenant"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "User",
      "description": "Standard user with basic permissions"
    }
  ],
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**UserRoleDto Schema:**

| Field       | Type   | Description              |
| ----------- | ------ | ------------------------ |
| id          | Guid   | Role's unique identifier |
| name        | string | Role name                |
| description | string | Role description         |

**Error Responses:**

**404 Not Found** - User doesn't exist:

```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**cURL Example:**

```bash
curl -X GET http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/roles \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 4. Create User

Creates a new user in the current tenant. **Admin only.**

**Endpoint:**

```
POST /api/v1/users
```

**Authorization:** `TenantAdmin` or `SuperAdmin` role required

**Request Body:**

```json
{
  "email": "newuser@example.com",
  "password": "SecurePassword123!",
  "fullName": "Alice Johnson",
  "initialRoles": ["User"]
}
```

**Request Schema:**

| Field        | Type     | Required | Description                             |
| ------------ | -------- | -------- | --------------------------------------- |
| email        | string   | Yes      | User's email address (must be unique)   |
| password     | string   | Yes      | User's password (min 8 characters)      |
| fullName     | string   | Yes      | User's full name                        |
| initialRoles | string[] | No       | Initial roles to assign (default: User) |

**Success Response (200 OK):**

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

**Error Responses:**

**403 Forbidden** - Insufficient permissions:

```json
{
  "success": false,
  "data": null,
  "message": "Access denied. Insufficient permissions.",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**409 Conflict** - Email already exists:

```json
{
  "success": false,
  "data": null,
  "message": "User with email 'newuser@example.com' already exists",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**422 Unprocessable Entity** - Validation failed:

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "Email": ["Email must be a valid email address"],
    "Password": ["Password must be at least 8 characters"],
    "FullName": ["Full name is required"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Validation Rules:**

- `Email`: Required, valid format, unique within tenant
- `Password`: Required, min 8 chars, uppercase, lowercase, digit
- `FullName`: Required, min 2 characters
- `InitialRoles`: Optional, must be valid role names

**cURL Example:**

```bash
curl -X POST http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePassword123!",
    "fullName": "Alice Johnson",
    "initialRoles": ["User"]
  }'
```

---

### 5. Update User

Updates an existing user's information. **Admin only.**

**Endpoint:**

```
PUT /api/v1/users/{id}
```

**Authorization:** `TenantAdmin` or `SuperAdmin` role required

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| id        | Guid | Yes      | User ID     |

**Request Body:**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "John Updated Doe",
  "email": "updated@example.com",
  "isActive": true
}
```

**Request Schema:**

| Field    | Type   | Required | Description                    |
| -------- | ------ | -------- | ------------------------------ |
| userId   | Guid   | Yes      | User ID (must match route ID)  |
| fullName | string | No       | Updated full name              |
| email    | string | No       | Updated email (must be unique) |
| isActive | bool   | No       | Updated active status          |

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "updated@example.com",
    "fullName": "John Updated Doe",
    "isActive": true,
    "updatedAt": "2026-05-15T19:24:15.994Z"
  },
  "message": "User updated successfully",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Error Responses:**

**400 Bad Request** - Route ID mismatch:

```json
{
  "success": false,
  "data": null,
  "message": "Route ID does not match command ID",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**404 Not Found** - User doesn't exist:

```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**409 Conflict** - Email already in use:

```json
{
  "success": false,
  "data": null,
  "message": "Email 'updated@example.com' is already in use",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**cURL Example:**

```bash
curl -X PUT http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "John Updated Doe",
    "email": "updated@example.com",
    "isActive": true
  }'
```

---

### 6. Delete User

Soft deletes a user (sets IsDeleted = true). **Admin only.**

**Endpoint:**

```
DELETE /api/v1/users/{id}
```

**Authorization:** `TenantAdmin` or `SuperAdmin` role required

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| id        | Guid | Yes      | User ID     |

**Example Request:**

```
DELETE /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": null,
  "message": "User deleted successfully",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Error Responses:**

**404 Not Found** - User doesn't exist:

```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**400 Bad Request** - Cannot delete self:

```json
{
  "success": false,
  "data": null,
  "message": "You cannot delete your own account",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Soft Delete Behavior:**

- User record remains in database with `IsDeleted = true`
- Deleted users cannot log in
- Deleted users are excluded from queries by default
- Audit trail is preserved

**cURL Example:**

```bash
curl -X DELETE http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 7. Assign Role to User

Assigns a role to a user. **Admin only.**

**Endpoint:**

```
POST /api/v1/users/{id}/roles
```

**Authorization:** `TenantAdmin` or `SuperAdmin` role required

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| id        | Guid | Yes      | User ID     |

**Request Body:**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roleId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d"
}
```

**Request Schema:**

| Field  | Type | Required | Description                   |
| ------ | ---- | -------- | ----------------------------- |
| userId | Guid | Yes      | User ID (must match route ID) |
| roleId | Guid | Yes      | Role ID to assign             |

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": null,
  "message": "Role assigned successfully",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Error Responses:**

**400 Bad Request** - Route ID mismatch:

```json
{
  "success": false,
  "data": null,
  "message": "Route ID does not match command ID",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**404 Not Found** - User or role doesn't exist:

```json
{
  "success": false,
  "data": null,
  "message": "User or role not found",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**409 Conflict** - Role already assigned:

```json
{
  "success": true,
  "data": null,
  "message": "Role already assigned (idempotent operation)",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Idempotent Behavior:**

Assigning a role that's already assigned returns success (not an error). This allows safe retry logic.

**cURL Example:**

```bash
curl -X POST http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/roles \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "roleId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d"
  }'
```

---

### 8. Remove Role from User

Removes a role from a user. **Admin only.**

**Endpoint:**

```
DELETE /api/v1/users/{id}/roles/{roleId}
```

**Authorization:** `TenantAdmin` or `SuperAdmin` role required

**Path Parameters:**

| Parameter | Type | Required | Description       |
| --------- | ---- | -------- | ----------------- |
| id        | Guid | Yes      | User ID           |
| roleId    | Guid | Yes      | Role ID to remove |

**Example Request:**

```
DELETE /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/roles/9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": null,
  "message": "Role removed successfully",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Error Responses:**

**404 Not Found** - User or role doesn't exist:

```json
{
  "success": false,
  "data": null,
  "message": "User or role not found",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Idempotent Behavior:**

Removing a role that's not assigned returns success (not an error).

**cURL Example:**

```bash
curl -X DELETE http://localhost:5000/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/roles/9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Authorization Matrix

| Endpoint                          | Anonymous | User | TenantAdmin | SuperAdmin |
| --------------------------------- | --------- | ---- | ----------- | ---------- |
| GET /users                        | ❌        | ✅   | ✅          | ✅         |
| GET /users/{id}                   | ❌        | ✅   | ✅          | ✅         |
| GET /users/{id}/roles             | ❌        | ✅   | ✅          | ✅         |
| POST /users                       | ❌        | ❌   | ✅          | ✅         |
| PUT /users/{id}                   | ❌        | ❌   | ✅          | ✅         |
| DELETE /users/{id}                | ❌        | ❌   | ✅          | ✅         |
| POST /users/{id}/roles            | ❌        | ❌   | ✅          | ✅         |
| DELETE /users/{id}/roles/{roleId} | ❌        | ❌   | ✅          | ✅         |

**Notes:**

- **Anonymous**: No authentication token required
- **User**: Standard user role, read-only access
- **TenantAdmin**: Full user management within their tenant
- **SuperAdmin**: Full user management across all tenants

---

## Tenant Isolation

All user operations are automatically scoped to the authenticated user's tenant:

1. **JWT Token Contains Tenant ID:**

   ```json
   {
     "tenant_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
   }
   ```

2. **Middleware Extracts Tenant Context:**

   ```csharp
   var tenantId = User.FindFirst("tenant_id")?.Value;
   HttpContext.Items["TenantId"] = tenantId;
   ```

3. **Repositories Filter by Tenant:**
   ```csharp
   var users = await _dbContext.Users
       .Where(u => u.TenantId == _tenantContext.TenantId)
       .ToListAsync();
   ```

**Result:** Users can only view/manage users within their own tenant. Cross-tenant access is impossible.

---

## Common Scenarios

### Scenario 1: Admin Creates New User

```
1. Admin logs in (receives token with TenantAdmin role)
2. Admin sends POST /api/v1/users with new user details
3. Backend validates admin role
4. Backend creates user in admin's tenant
5. Backend returns created user details
6. Admin can see new user in GET /api/v1/users list
```

### Scenario 2: User Views Colleague Profile

```
1. User logs in (receives token with User role)
2. User sends GET /api/v1/users/{colleagueId}
3. Backend validates user is authenticated
4. Backend checks colleague belongs to same tenant
5. Backend returns colleague details
```

### Scenario 3: Admin Assigns Role

```
1. Admin identifies user needing TenantAdmin role
2. Admin sends POST /api/v1/users/{userId}/roles with roleId
3. Backend validates admin has permission
4. Backend assigns role to user
5. User's next login includes new role in JWT claims
```

---

## Pagination Best Practices

### 1. Default Values

If pagination parameters are omitted, defaults apply:

- `pageNumber`: 1
- `pageSize`: 10

### 2. Maximum Page Size

To prevent performance issues, `pageSize` is capped at 100.

### 3. Navigation Metadata

Response includes navigation flags:

```json
{
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

Use these to enable/disable "Previous" and "Next" buttons.

### 4. Total Count

`totalCount` enables client-side page number calculation:

```javascript
const totalPages = Math.ceil(totalCount / pageSize);
```

---

## Search and Filtering

### Search Term

The `searchTerm` parameter searches across:

- Email (case-insensitive)
- Full name (case-insensitive)

**Example:**

```
GET /api/v1/users?searchTerm=john
```

Matches:

- john.doe@example.com
- Johnny Smith
- Alice Johnson

### Active Filter

Filter by user active status:

```
GET /api/v1/users?isActive=true  // Only active users
GET /api/v1/users?isActive=false // Only inactive users
GET /api/v1/users                // All users
```

### Sorting

Customize result order:

```
GET /api/v1/users?orderBy=Email&orderDirection=Asc
```

Supported `orderBy` values:

- `CreatedAt` (default)
- `Email`
- `FullName`
- `LastLoginAt`

---

## Testing

### Integration Test Example

```csharp
[Fact]
public async Task GetUsers_AsAuthenticatedUser_ReturnsPaginatedList()
{
    // Arrange
    var client = await _factory.GetAuthenticatedClientAsync("user@tenant1.com");

    // Act
    var response = await client.GetAsync("/api/v1/users?pageNumber=1&pageSize=10");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content
        .ReadFromJsonAsync<ApiResponse<PaginatedResponse<UserDto>>>();
    result.Success.Should().BeTrue();
    result.Data.Items.Should().NotBeEmpty();
}

[Fact]
public async Task CreateUser_AsNonAdmin_ReturnsForbidden()
{
    // Arrange
    var client = await _factory.GetAuthenticatedClientAsync("user@tenant1.com");
    var request = new
    {
        email = "newuser@example.com",
        password = "SecurePass123!",
        fullName = "Test User"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/v1/users", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

---

## Summary

- **8 Endpoints**: Complete CRUD + role management
- **Role-Based Access**: Read for all authenticated, write for admins only
- **Pagination**: Efficient handling of large user lists
- **Tenant Isolation**: Automatic scoping to current tenant
- **Soft Deletes**: Preserve audit trail
- **Idempotent Operations**: Safe retry logic for role assignments
- **Comprehensive Validation**: Field-level error messages

User management endpoints provide the foundation for multi-tenant user administration.
