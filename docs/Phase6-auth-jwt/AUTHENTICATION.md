# Authentication & JWT Implementation

**Project**: dotnet-saas-multitenant-api  
**Created**: May 14, 2026  
**Phase**: Phase 6 - Authentication & JWT

---

## Overview

This document describes the JWT-based authentication implementation for the multi-tenant SaaS API. The authentication system is designed to secure API endpoints while maintaining tenant isolation and providing role-based access control.

---

## Architecture

### Components

1. **JWT Token Generator** (`JwtTokenGenerator`)
   - Generates access tokens and refresh tokens
   - Embeds user, tenant, and role claims into tokens
   - Configurable expiration times

2. **Password Hasher** (`PasswordHasher`)
   - Uses BCrypt with work factor 12
   - One-way hashing for secure password storage
   - Verification method for login authentication

3. **Refresh Token System**
   - Persistent refresh tokens stored in database
   - Support for token revocation
   - Automatic cleanup of expired tokens

4. **Current User Service** (`CurrentUserService`)
   - Extracts authenticated user information from JWT claims
   - Provides user ID, email, and tenant ID to application layer
   - Scoped service that accesses HttpContext

---

## JWT Token Structure

### Access Token Claims

The access token contains the following claims:

```json
{
  "sub": "user-guid", // User ID (Subject)
  "email": "user@example.com", // User email
  "tenantId": "tenant-guid", // Tenant ID for isolation
  "jti": "token-guid", // JWT ID (unique token identifier)
  "role": ["Admin", "User"], // User roles (array)
  "iss": "dotnet-saas-api", // Issuer
  "aud": "dotnet-saas-client", // Audience
  "exp": 1234567890, // Expiration timestamp
  "iat": 1234567890 // Issued at timestamp
}
```

### Token Settings

Configured in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-characters-long-change-in-production",
    "Issuer": "dotnet-saas-api",
    "Audience": "dotnet-saas-client",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Important Security Notes:**

- The `SecretKey` must be at least 32 characters long
- Use a cryptographically secure random string in production
- Store secrets in environment variables or Azure Key Vault, not in `appsettings.json`

---

## Token Generation Process

### Access Token Generation

1. User authenticates with email and password
2. Password is verified using BCrypt
3. User roles are retrieved from the database
4. JWT token is generated with user, tenant, and role claims
5. Token is signed using HMAC-SHA256 algorithm
6. Token is returned to client with expiration time

### Refresh Token Generation

1. Cryptographically secure random 64-byte token is generated
2. Token is Base64 encoded
3. Token is stored in database with user ID and expiration date
4. Token is returned to client alongside access token

---

## Token Validation Process

### Authentication Flow

1. Client sends request with `Authorization: Bearer <token>` header
2. JWT Bearer authentication middleware extracts the token
3. Token signature is validated using the secret key
4. Token expiration is checked (with zero clock skew)
5. Issuer and audience are validated
6. If valid, user claims are extracted and available via `ICurrentUserService`

### Validation Parameters

```csharp
ValidateIssuer = true
ValidateAudience = true
ValidateLifetime = true
ValidateIssuerSigningKey = true
ClockSkew = TimeSpan.Zero  // No tolerance for expired tokens
```

---

## Refresh Token Flow

### Token Refresh Process

1. Client sends expired/expiring access token + valid refresh token
2. System validates refresh token:
   - Token exists in database
   - Token is not revoked
   - Token is not expired
   - Token belongs to the user in the access token
3. If valid:
   - New access token is generated
   - Optionally, new refresh token is generated (token rotation)
   - Old refresh token is revoked (if rotation is enabled)
4. New tokens are returned to client

### Token Revocation

Refresh tokens can be revoked in the following scenarios:

- User logout (revoke all tokens for user)
- Password change (revoke all tokens for user)
- Suspicious activity detected
- Manual revocation by administrator

---

## Password Hashing

### Hashing Algorithm

- **Algorithm**: BCrypt
- **Work Factor**: 12 (2^12 = 4096 rounds)
- **Library**: BCrypt.Net-Next v4.1.0

### Password Requirements

Passwords must meet the following criteria:

- Minimum 8 characters
- At least 1 uppercase letter (A-Z)
- At least 1 lowercase letter (a-z)
- At least 1 number (0-9)
- At least 1 special character (!@#$%^&\*()\_+-=[]{};\':\"\\|,.<>/?)

### Password Validation

The `PasswordValidator` class provides validation:

```csharp
if (!PasswordValidator.IsValid(password, out var errors))
{
    // Handle validation errors
    // errors contains list of validation failure messages
}
```

---

## Database Schema

### RefreshToken Table

```sql
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    token VARCHAR(500) NOT NULL UNIQUE,
    expires_at_utc TIMESTAMP NOT NULL,
    revoked_at_utc TIMESTAMP NULL,
    created_at_utc TIMESTAMP NOT NULL,
    updated_at_utc TIMESTAMP NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_user_expires ON refresh_tokens(user_id, expires_at_utc);
```

---

## Service Registration

### Infrastructure Layer

All authentication services are registered in `Infrastructure/DependencyInjection.cs`:

```csharp
// JWT Settings
services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

// Token Generation
services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Password Hashing
services.AddScoped<IPasswordHashingService, PasswordHasher>();

// Current User Service
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();

// Repository
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// JWT Authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* configuration */);

services.AddAuthorization();
```

---

## Middleware Pipeline

The authentication middleware must be positioned correctly in the pipeline:

```csharp
app.UseHttpsRedirection();
app.UseTenantResolution();     // Tenant context must be resolved first
app.UseAuthentication();       // JWT authentication
app.UseAuthorization();        // Authorization policies
app.MapControllers();
```

**Order is critical:**

1. Tenant resolution must happen before authentication
2. Authentication must happen before authorization
3. Both must happen before controller endpoints

---

## Usage Examples

### Protecting an Endpoint

```csharp
[Authorize]  // Requires authentication
[HttpGet("protected")]
public IActionResult GetProtectedResource()
{
    var userId = _currentUserService.UserId;
    var tenantId = _currentUserService.TenantId;
    // ...
}
```

### Role-Based Authorization

```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly()
{
    // Only users with Admin role can access
}
```

### Accessing Current User

```csharp
public class MyHandler
{
    private readonly ICurrentUserService _currentUser;

    public MyHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MyCommand request)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure(Error.Unauthorized());

        var userId = _currentUser.UserId!.Value;
        var tenantId = _currentUser.TenantId!.Value;
        // ...
    }
}
```

---

## Security Considerations

### Best Practices

1. **Secret Key Management**
   - Never commit secret keys to source control
   - Use environment variables or secret management services
   - Rotate keys periodically in production

2. **Token Expiration**
   - Keep access token expiration short (15-30 minutes)
   - Longer refresh token expiration (7-30 days)
   - Implement token rotation for refresh tokens

3. **Password Storage**
   - Never store plaintext passwords
   - Always use BCrypt or Argon2 for hashing
   - Minimum work factor of 12 for BCrypt

4. **Transport Security**
   - Always use HTTPS in production
   - Set secure cookie flags for refresh tokens (if using cookies)

5. **Token Validation**
   - Validate all claims (issuer, audience, expiration)
   - Use zero clock skew to prevent time-based attacks
   - Implement token blacklisting for high-security scenarios

### Common Vulnerabilities

| Vulnerability  | Mitigation                                               |
| -------------- | -------------------------------------------------------- |
| Token theft    | Use HTTPS, short expiration, refresh tokens              |
| Replay attacks | Include jti claim, implement token rotation              |
| Brute force    | Rate limiting, account lockout, strong passwords         |
| XSS attacks    | Don't store tokens in localStorage, use httpOnly cookies |
| CSRF           | Use SameSite cookies, anti-CSRF tokens                   |

---

## Testing

### Unit Tests

The following components should have unit tests:

1. **JwtTokenGenerator**
   - Token generation with correct claims
   - Token expiration calculation
   - Refresh token generation randomness

2. **PasswordHasher**
   - Password hashing produces unique hashes
   - Verification works correctly
   - Incorrect passwords fail verification

3. **PasswordValidator**
   - Each validation rule is tested
   - Edge cases (empty, null, special characters)

4. **CurrentUserService**
   - Claims extraction from HttpContext
   - Null handling when not authenticated

### Integration Tests

1. **Authentication Flow**
   - Login with valid credentials
   - Login with invalid credentials
   - Token refresh flow
   - Token revocation

2. **Authorization**
   - Protected endpoints reject unauthenticated requests
   - Role-based access control
   - Tenant isolation

---

## Troubleshooting

### Common Issues

**Issue**: 401 Unauthorized on valid token

- **Check**: Token signature validation
- **Check**: Issuer/Audience configuration match
- **Check**: Clock skew and token expiration

**Issue**: Claims not found in CurrentUserService

- **Check**: Token generation includes required claims
- **Check**: Claim types match between generation and extraction
- **Check**: HttpContext is available (not in background tasks)

**Issue**: Password verification fails

- **Check**: Password is being hashed before storage
- **Check**: BCrypt work factor matches
- **Check**: No trimming/modification of hashed password

---

## Future Enhancements

1. **Token Rotation**: Implement automatic refresh token rotation
2. **Token Blacklist**: Add revoked token cache (Redis)
3. **Multi-Factor Authentication**: Add MFA support
4. **OAuth/OpenID Connect**: Support external identity providers
5. **Refresh Token Families**: Track token lineage for better security
6. **Adaptive Authentication**: Context-aware authentication requirements

---

## References

- [JWT RFC 7519](https://tools.ietf.org/html/rfc7519)
- [BCrypt](https://en.wikipedia.org/wiki/Bcrypt)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Microsoft ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
