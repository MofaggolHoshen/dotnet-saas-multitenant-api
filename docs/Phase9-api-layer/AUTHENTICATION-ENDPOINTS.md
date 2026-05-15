# Authentication Endpoints

This document provides detailed specifications for all authentication-related API endpoints in Phase 9.

---

## Overview

The `AuthController` exposes three public endpoints for user authentication:

- **Login**: Authenticate with email/password, receive JWT tokens
- **Register**: Create a new user account
- **Refresh Token**: Obtain new access token using refresh token

All authentication endpoints are **anonymous** (no authentication required) and use the `[AllowAnonymous]` attribute.

---

## Base Route

```
/api/v1/auth
```

---

## Endpoints

### 1. Login

Authenticates a user and returns JWT access/refresh tokens.

**Endpoint:**

```
POST /api/v1/auth/login
```

**Authorization:** Anonymous

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Request Schema:**

| Field    | Type   | Required | Description                                        |
| -------- | ------ | -------- | -------------------------------------------------- |
| email    | string | Yes      | User's email address                               |
| password | string | Yes      | User's password (plain text, encrypted in transit) |
| tenantId | Guid   | Yes      | ID of the tenant the user belongs to               |

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
    "refreshToken": "dGVzdC1yZWZyZXNoLXRva2VuLWV4YW1wbGUtMTIzNDU2Nzg5MA==",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Response Schema:**

| Field             | Type   | Description                                   |
| ----------------- | ------ | --------------------------------------------- |
| data.accessToken  | string | JWT access token (1 hour expiration)          |
| data.refreshToken | string | Refresh token for obtaining new access tokens |
| data.expiresIn    | int    | Access token lifetime in seconds (3600 = 1h)  |
| data.tokenType    | string | Token type, always "Bearer"                   |

**Error Responses:**

**401 Unauthorized** - Invalid credentials:

```json
{
  "success": false,
  "data": null,
  "message": "Invalid email or password",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**400 Bad Request** - Tenant not found:

```json
{
  "success": false,
  "data": null,
  "message": "Tenant not found",
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
    "Email": ["Email is required", "Email must be a valid email address"],
    "Password": ["Password is required"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Validation Rules:**

- `Email`: Required, valid email format
- `Password`: Required, minimum 1 character
- `TenantId`: Required, valid GUID

**JWT Token Claims:**

The returned access token contains the following claims:

```json
{
  "sub": "7c9e6679-7425-40de-944b-e07fc1f90ae7", // User ID
  "email": "user@example.com", // Email
  "name": "John Doe", // Full name
  "tenant_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", // Tenant ID
  "role": ["User", "TenantAdmin"], // Roles
  "nbf": 1652638855, // Not before
  "exp": 1652642455, // Expiration (1 hour)
  "iat": 1652638855, // Issued at
  "iss": "DotnetSaasMultitenantApi", // Issuer
  "aud": "DotnetSaasMultitenantApi" // Audience
}
```

**cURL Example:**

```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@tenant1.com",
    "password": "Admin123!",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

**C# Example:**

```csharp
var loginCommand = new LoginCommand(
    Email: "admin@tenant1.com",
    Password: "Admin123!",
    TenantId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
);

var response = await httpClient.PostAsJsonAsync("/api/v1/auth/login", loginCommand);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

if (result.Success)
{
    var accessToken = result.Data.AccessToken;
    // Store token for subsequent requests
}
```

**JavaScript Example:**

```javascript
const response = await fetch("http://localhost:5000/api/v1/auth/login", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email: "admin@tenant1.com",
    password: "Admin123!",
    tenantId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  }),
});

const result = await response.json();
if (result.success) {
  localStorage.setItem("accessToken", result.data.accessToken);
  localStorage.setItem("refreshToken", result.data.refreshToken);
}
```

---

### 2. Register

Creates a new user account in the specified tenant.

**Endpoint:**

```
POST /api/v1/auth/register
```

**Authorization:** Anonymous

**Request Body:**

```json
{
  "email": "newuser@example.com",
  "password": "SecurePassword123!",
  "fullName": "John Doe",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Request Schema:**

| Field    | Type   | Required | Description                           |
| -------- | ------ | -------- | ------------------------------------- |
| email    | string | Yes      | User's email address (must be unique) |
| password | string | Yes      | User's password (min 8 characters)    |
| fullName | string | Yes      | User's full name                      |
| tenantId | Guid   | Yes      | ID of the tenant to create user in    |

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "email": "newuser@example.com",
    "fullName": "John Doe",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "message": "User registered successfully",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Response Schema:**

| Field         | Type   | Description                  |
| ------------- | ------ | ---------------------------- |
| data.userId   | Guid   | ID of the newly created user |
| data.email    | string | User's email address         |
| data.fullName | string | User's full name             |
| data.tenantId | Guid   | Tenant the user belongs to   |

**Error Responses:**

**409 Conflict** - Email already exists:

```json
{
  "success": false,
  "data": null,
  "message": "User with this email already exists",
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**400 Bad Request** - Tenant not found:

```json
{
  "success": false,
  "data": null,
  "message": "Tenant not found",
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
    "Password": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter",
      "Password must contain at least one lowercase letter",
      "Password must contain at least one digit"
    ],
    "FullName": ["Full name is required"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Validation Rules:**

- `Email`:
  - Required
  - Valid email format
  - Must be unique within tenant
- `Password`:
  - Required
  - Minimum 8 characters
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one digit
- `FullName`:
  - Required
  - Minimum 2 characters
- `TenantId`:
  - Required
  - Valid GUID
  - Tenant must exist

**cURL Example:**

```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePassword123!",
    "fullName": "John Doe",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

**C# Example:**

```csharp
var registerCommand = new RegisterCommand(
    Email: "newuser@example.com",
    Password: "SecurePassword123!",
    FullName: "John Doe",
    TenantId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
);

var response = await httpClient.PostAsJsonAsync("/api/v1/auth/register", registerCommand);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>();

if (result.Success)
{
    Console.WriteLine($"User created with ID: {result.Data.UserId}");
}
```

**JavaScript Example:**

```javascript
const response = await fetch("http://localhost:5000/api/v1/auth/register", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email: "newuser@example.com",
    password: "SecurePassword123!",
    fullName: "John Doe",
    tenantId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  }),
});

const result = await response.json();
if (result.success) {
  console.log("Registration successful!", result.data);
}
```

---

### 3. Refresh Token

Obtains a new access token using a valid refresh token.

**Endpoint:**

```
POST /api/v1/auth/refresh
```

**Authorization:** Anonymous

**Request Body:**

```json
{
  "refreshToken": "dGVzdC1yZWZyZXNoLXRva2VuLWV4YW1wbGUtMTIzNDU2Nzg5MA=="
}
```

**Request Schema:**

| Field        | Type   | Required | Description                    |
| ------------ | ------ | -------- | ------------------------------ |
| refreshToken | string | Yes      | Valid refresh token from login |

**Success Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "bmV3LXJlZnJlc2gtdG9rZW4tZXhhbXBsZS0xMjM0NTY3ODkw",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "message": null,
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Response Schema:**

| Field             | Type   | Description                                  |
| ----------------- | ------ | -------------------------------------------- |
| data.accessToken  | string | New JWT access token (1 hour expiration)     |
| data.refreshToken | string | New refresh token (old token is invalidated) |
| data.expiresIn    | int    | Access token lifetime in seconds             |
| data.tokenType    | string | Token type, always "Bearer"                  |

**Error Responses:**

**401 Unauthorized** - Invalid or expired refresh token:

```json
{
  "success": false,
  "data": null,
  "message": "Invalid or expired refresh token",
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
    "RefreshToken": ["Refresh token is required"]
  },
  "timestamp": "2026-05-15T19:24:15.994Z"
}
```

**Validation Rules:**

- `RefreshToken`:
  - Required
  - Must be a valid, non-expired refresh token
  - Must not have been revoked

**Token Rotation:**

When a refresh token is used:

1. Old refresh token is **invalidated** (single-use)
2. New access token is generated
3. New refresh token is generated
4. Client must store **both** new tokens

This prevents replay attacks and ensures tokens remain fresh.

**cURL Example:**

```bash
curl -X POST http://localhost:5000/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "dGVzdC1yZWZyZXNoLXRva2VuLWV4YW1wbGUtMTIzNDU2Nzg5MA=="
  }'
```

**C# Example:**

```csharp
var refreshCommand = new RefreshTokenCommand(
    RefreshToken: storedRefreshToken
);

var response = await httpClient.PostAsJsonAsync("/api/v1/auth/refresh", refreshCommand);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>();

if (result.Success)
{
    // Update stored tokens
    accessToken = result.Data.AccessToken;
    refreshToken = result.Data.RefreshToken;
}
```

**JavaScript Example:**

```javascript
const refreshToken = localStorage.getItem("refreshToken");

const response = await fetch("http://localhost:5000/api/v1/auth/refresh", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ refreshToken }),
});

const result = await response.json();
if (result.success) {
  localStorage.setItem("accessToken", result.data.accessToken);
  localStorage.setItem("refreshToken", result.data.refreshToken);
}
```

---

## Authentication Flow

### Initial Login Flow

```
1. User submits email, password, tenantId
   ↓
2. Server validates credentials
   ↓
3. Server generates access token (JWT, 1h expiration)
   ↓
4. Server generates refresh token (random, 7d expiration)
   ↓
5. Server stores refresh token in database
   ↓
6. Server returns both tokens to client
   ↓
7. Client stores tokens (localStorage/secure cookie)
```

### Authenticated Request Flow

```
1. Client sends request with Authorization header
   Authorization: Bearer <access-token>
   ↓
2. Server validates JWT signature
   ↓
3. Server checks token expiration
   ↓
4. Server extracts user/tenant context from claims
   ↓
5. Server processes request
```

### Token Refresh Flow

```
1. Client detects access token expired (401 response)
   ↓
2. Client sends refresh token to /auth/refresh
   ↓
3. Server validates refresh token (not expired, not revoked)
   ↓
4. Server invalidates old refresh token
   ↓
5. Server generates new access token + new refresh token
   ↓
6. Server returns both new tokens
   ↓
7. Client stores new tokens
   ↓
8. Client retries original request with new access token
```

---

## Security Considerations

### 1. Password Security

- Passwords are **never stored in plain text**
- BCrypt hashing with salt (configured in Phase 6)
- Minimum password strength enforced via validation

### 2. Token Security

**Access Token:**

- Short-lived (1 hour)
- Signed with HMAC-SHA256
- Cannot be revoked (stateless)
- Contains minimal claims

**Refresh Token:**

- Longer-lived (7 days)
- Stored in database for revocation
- Single-use (invalidated on refresh)
- Random, cryptographically secure

### 3. HTTPS Required

**⚠️ Production Requirement:**

- All authentication endpoints **must** use HTTPS
- Tokens transmitted in plain HTTP are vulnerable to interception
- Configure SSL/TLS certificate in production

### 4. Rate Limiting

Consider implementing rate limiting on authentication endpoints:

```csharp
// Example: Max 5 login attempts per minute per IP
[RateLimit(MaxRequests = 5, WindowSeconds = 60)]
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginCommand command)
    => FromResult(await Sender.Send(command));
```

### 5. Tenant Isolation

- Users can only authenticate to their assigned tenant
- Cross-tenant authentication is blocked
- Tenant context is embedded in JWT claims

---

## Common Scenarios

### Scenario 1: First-Time User Registration

```
1. User visits registration page
2. User enters email, password, full name
3. Frontend sends POST /api/v1/auth/register
4. Backend creates user account
5. Frontend redirects to login page
6. User logs in with new credentials
```

### Scenario 2: Returning User Login

```
1. User visits login page
2. User enters email and password
3. Frontend sends POST /api/v1/auth/login
4. Backend returns access + refresh tokens
5. Frontend stores tokens
6. Frontend redirects to dashboard
7. All subsequent requests include Authorization header
```

### Scenario 3: Access Token Expiration

```
1. User makes API request after 1 hour (token expired)
2. API returns 401 Unauthorized
3. Frontend detects 401, sends refresh token to /auth/refresh
4. Backend returns new tokens
5. Frontend stores new tokens
6. Frontend retries original request with new access token
7. Request succeeds
```

### Scenario 4: Logout

```
1. User clicks "Logout"
2. Frontend sends refresh token to backend (optional revocation endpoint)
3. Backend invalidates refresh token
4. Frontend clears tokens from storage
5. Frontend redirects to login page
```

---

## Testing

### Postman Collection

**Login Request:**

```
POST http://localhost:5000/api/v1/auth/login
Content-Type: application/json

{
  "email": "admin@tenant1.com",
  "password": "Admin123!",
  "tenantId": "{{tenantId}}"
}
```

**Register Request:**

```
POST http://localhost:5000/api/v1/auth/register
Content-Type: application/json

{
  "email": "test{{$randomInt}}@example.com",
  "password": "SecurePass123!",
  "fullName": "Test User",
  "tenantId": "{{tenantId}}"
}
```

**Refresh Request:**

```
POST http://localhost:5000/api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "{{refreshToken}}"
}
```

### Integration Test

```csharp
[Fact]
public async Task Login_ValidCredentials_ReturnsTokens()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new
    {
        email = "admin@tenant1.com",
        password = "Admin123!",
        tenantId = _tenantId
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    result.Success.Should().BeTrue();
    result.Data.AccessToken.Should().NotBeNullOrEmpty();
    result.Data.RefreshToken.Should().NotBeNullOrEmpty();
}
```

---

## Summary

- **3 Anonymous Endpoints**: Login, Register, Refresh Token
- **JWT-Based Authentication**: Secure, stateless access tokens
- **Refresh Token Rotation**: Single-use refresh tokens for enhanced security
- **Validation**: Comprehensive input validation with detailed error messages
- **Tenant Isolation**: All authentication is tenant-scoped
- **Password Security**: BCrypt hashing, strength requirements
- **Standardized Responses**: Consistent `ApiResponse<T>` wrapper

Authentication is the foundation for all protected endpoints in subsequent phases.
