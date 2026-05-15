# Phase 6 – Authentication & JWT

**Phase**: 6  
**Status**: 🟢 Completed  
**Priority**: 🔴 Critical  
**Depends on**: Phase 5 (Multi-Tenancy Infrastructure)

---

## Table of Contents

1. [Overview](#overview)
2. [Files Added](#files-added)
3. [Files Modified](#files-modified)
4. [Component Breakdown](#component-breakdown)
   - [JwtSettings](#1-jwtsettings)
   - [JwtTokenGenerator](#2-jwttokengenerator)
   - [PasswordHasher](#3-passwordhasher)
   - [PasswordValidator](#4-passwordvalidator)
   - [RefreshToken Entity](#5-refreshtoken-entity)
   - [RefreshToken EF Configuration](#6-refreshtoken-ef-configuration)
   - [RefreshToken Repository](#7-refreshtokenrepository)
   - [CurrentUserService](#8-currentuserservice)
   - [Infrastructure DI Registration](#9-infrastructure-di-registration)
   - [Program.cs Middleware](#10-programcs-middleware)
   - [Database Migration](#11-database-migration)
5. [JWT Token Structure](#jwt-token-structure)
6. [Authentication Flow](#authentication-flow)
7. [Middleware Pipeline Order](#middleware-pipeline-order)
8. [Configuration](#configuration)
9. [Design Decisions](#design-decisions)

---

## Overview

Phase 6 introduced all authentication and JWT infrastructure. The goal was to build a stateless token-based authentication system that carries **tenant identity** inside the token, integrates with the existing multi-tenancy infrastructure from Phase 5, and exposes current user context to the Application layer without coupling it to the HTTP stack.

Five new components were added to `Infrastructure/Identity`, one domain entity was added, one new repository interface and implementation were created, and the middleware pipeline was updated to activate authentication.

---

## Files Added

| File                                                                     | Layer          | Purpose                                              |
| ------------------------------------------------------------------------ | -------------- | ---------------------------------------------------- |
| `Infrastructure/Identity/JwtSettings.cs`                                 | Infrastructure | Typed options model for JWT configuration            |
| `Infrastructure/Identity/JwtTokenGenerator.cs`                           | Infrastructure | Generates access tokens and refresh tokens           |
| `Infrastructure/Identity/PasswordHasher.cs`                              | Infrastructure | BCrypt-based password hashing and verification       |
| `Infrastructure/Identity/CurrentUserService.cs`                          | Infrastructure | Extracts authenticated user claims from HTTP context |
| `Application/Common/Validators/PasswordValidator.cs`                     | Application    | Regex-based password strength validation             |
| `Application/Common/Interfaces/IRefreshTokenRepository.cs`               | Application    | Contract for refresh token persistence               |
| `Domain/Entities/RefreshToken.cs`                                        | Domain         | Refresh token aggregate with lifecycle state         |
| `Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` | Infrastructure | EF Core table mapping for `RefreshToken`             |
| `Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs`      | Infrastructure | Concrete refresh token repository                    |
| `docs/AUTHENTICATION.md`                                                 | Docs           | Full authentication reference documentation          |

---

## Files Modified

| File                                                        | Change                                                                         |
| ----------------------------------------------------------- | ------------------------------------------------------------------------------ |
| `source/WebAPI/appsettings.json`                            | Added `Jwt` configuration section                                              |
| `source/Infrastructure/DependencyInjection.cs`              | Registered all auth services; added `AddAuthenticationServices` private method |
| `source/Infrastructure/Persistence/ApplicationDbContext.cs` | Added `RefreshTokensSet` DbSet                                                 |
| `source/WebAPI/Program.cs`                                  | Added `UseAuthentication()` before `UseAuthorization()`                        |

---

## Component Breakdown

### 1. JwtSettings

**File**: `Infrastructure/Identity/JwtSettings.cs`

A plain options class bound to the `"Jwt"` section in `appsettings.json` using the .NET Options pattern.

```csharp
public sealed class JwtSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; init; }
    public int RefreshTokenExpirationDays { get; init; }
}
```

Registered via `services.Configure<JwtSettings>(configuration.GetSection("Jwt"))` and injected through `IOptions<JwtSettings>`.

---

### 2. JwtTokenGenerator

**File**: `Infrastructure/Identity/JwtTokenGenerator.cs`  
**Interface**: `IJwtTokenGenerator`

Responsible for two token types:

**Access Token** (`GenerateAccessToken`):

- Signs with HMAC-SHA256 using the secret key
- Embeds the following claims:

| Claim      | Value                                |
| ---------- | ------------------------------------ |
| `sub`      | User GUID                            |
| `email`    | User email address                   |
| `tenantId` | Tenant GUID (custom claim)           |
| `jti`      | Unique token ID (new GUID per token) |
| `role`     | Each role added as a separate claim  |

- Expiration set to `AccessTokenExpirationMinutes` from settings

**Refresh Token** (`GenerateRefreshToken`):

- Generates 64 cryptographically random bytes via `RandomNumberGenerator.GetBytes(64)`
- Returns a Base64-encoded string — no claims, pure opaque token

```csharp
public string GenerateRefreshToken()
    => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
```

---

### 3. PasswordHasher

**File**: `Infrastructure/Identity/PasswordHasher.cs`  
**Implements**: `IPasswordHashingService` (Domain layer interface)

Uses **BCrypt.Net-Next** with a work factor of **12** (2^12 = 4096 rounds). This makes brute-force attacks computationally expensive.

```csharp
public string Hash(string plainTextPassword)
    => BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor: 12);

public bool Verify(string plainTextPassword, string passwordHash)
    => BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
```

The interface `IPasswordHashingService` lives in the Domain layer so domain services can depend on it without referencing Infrastructure.

---

### 4. PasswordValidator

**File**: `Application/Common/Validators/PasswordValidator.cs`

A static partial class using .NET source-generated regex (compile-time, zero-allocation). Validates that a password meets the following rules:

| Rule              | Requirement                     |
| ----------------- | ------------------------------- |
| Length            | Minimum 8 characters            |
| Uppercase         | At least one `A-Z`              |
| Lowercase         | At least one `a-z`              |
| Digit             | At least one `0-9`              |
| Special character | At least one of `!@#$%^&*()...` |

Returns `bool` and populates an `out List<string> errors` with all failures (not just the first), enabling the caller to return all validation messages at once.

```csharp
if (!PasswordValidator.IsValid(password, out var errors))
{
    // errors contains all failed rules
}
```

---

### 5. RefreshToken Entity

**File**: `Domain/Entities/RefreshToken.cs`  
**Base**: `BaseEntity` (not `AggregateRoot` — no domain events needed)

Encapsulates the full lifecycle of a refresh token:

```
Created → Active → [Expired | Revoked]
```

Key computed properties:

| Property    | Logic                             |
| ----------- | --------------------------------- |
| `IsExpired` | `DateTime.UtcNow >= ExpiresAtUtc` |
| `IsRevoked` | `RevokedAtUtc.HasValue`           |
| `IsActive`  | `!IsExpired && !IsRevoked`        |

Private constructor enforces creation through the static factory:

```csharp
public static RefreshToken Create(Guid userId, string token, DateTime expiresAtUtc)
    => new(userId, token, expiresAtUtc);
```

`Revoke()` is idempotent — calling it on an already-revoked token is a no-op.

---

### 6. RefreshToken EF Configuration

**File**: `Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`

Maps to the `refresh_tokens` table with snake_case column names, consistent with the rest of the project.

Notable mapping decisions:

- `Token` column has a **unique index** — tokens must be globally unique
- Computed properties (`IsExpired`, `IsRevoked`, `IsActive`) are **ignored** by EF — they are calculated in memory
- Additional indexes: `user_id`, and composite `(user_id, expires_at_utc)` for efficient active-token queries

---

### 7. RefreshTokenRepository

**File**: `Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs`  
**Interface**: `Application/Common/Interfaces/IRefreshTokenRepository.cs`

| Method                         | Purpose                                                         |
| ------------------------------ | --------------------------------------------------------------- |
| `GetByTokenAsync`              | Look up a token string for validation                           |
| `GetByIdAsync`                 | Look up by primary key                                          |
| `GetActiveTokensByUserIdAsync` | Find all non-expired, non-revoked tokens for a user             |
| `AddAsync`                     | Persist a new refresh token                                     |
| `UpdateAsync`                  | Persist changes after revocation                                |
| `RevokeAllUserTokensAsync`     | Revoke every active token for a user (logout / password change) |
| `DeleteExpiredTokensAsync`     | Housekeeping — remove tokens past expiry                        |

The interface is defined in the **Application** layer, keeping the domain/application layers free from Infrastructure dependencies.

---

### 8. CurrentUserService

**File**: `Infrastructure/Identity/CurrentUserService.cs`  
**Implements**: `ICurrentUserService` (Application layer interface)

Reads the authenticated user's identity from the current `HttpContext` claims principal. Registered as **scoped** so it binds to the HTTP request lifetime.

```csharp
public Guid? UserId
    => TryParseGuid(user.FindFirstValue(JwtRegisteredClaimNames.Sub)
    ?? user.FindFirstValue(ClaimTypes.NameIdentifier));

public Guid? TenantId
    => TryParseGuid(user.FindFirstValue("tenantId"));
```

Falls back gracefully — all properties return `null` when the user is not authenticated, so handlers can check `IsAuthenticated` before accessing claims.

The `tenantId` claim uses the custom claim name set during token generation, tying `CurrentUserService` directly to the token structure defined in `JwtTokenGenerator`.

---

### 9. Infrastructure DI Registration

**File**: `Infrastructure/DependencyInjection.cs`

Authentication setup was extracted into a private `AddAuthenticationServices` method to keep `AddInfrastructureServices` readable. Services registered:

```
IOptions<JwtSettings>          → bound from "Jwt" config section
IJwtTokenGenerator             → JwtTokenGenerator        (Scoped)
IPasswordHashingService        → PasswordHasher           (Scoped)
IHttpContextAccessor           → built-in                 (Singleton)
ICurrentUserService            → CurrentUserService       (Scoped)
IRefreshTokenRepository        → RefreshTokenRepository   (Scoped)
AddAuthentication(JwtBearer)   → validates incoming tokens
AddAuthorization               → policy engine
```

JWT validation parameters:

| Parameter                | Value                          |
| ------------------------ | ------------------------------ |
| ValidateIssuer           | `true`                         |
| ValidateAudience         | `true`                         |
| ValidateLifetime         | `true`                         |
| ValidateIssuerSigningKey | `true`                         |
| ClockSkew                | `TimeSpan.Zero` (no tolerance) |

Zero clock skew means tokens expire exactly when `exp` says — no grace period.

---

### 10. Program.cs Middleware

**File**: `source/WebAPI/Program.cs`

`UseAuthentication()` was added before `UseAuthorization()`:

```csharp
app.UseTenantResolution();    // Phase 5 — resolve tenant from request header/claim
app.UseAuthentication();      // Phase 6 — validate JWT, populate ClaimsPrincipal
app.UseAuthorization();       // enforce [Authorize] policies
app.MapControllers();
```

Order is load-bearing — tenant resolution must happen before authentication so the tenant context is available during the request.

---

### 11. Database Migration

**Migration name**: `AddRefreshTokens`  
**Generated via**: `dotnet ef migrations add AddRefreshTokens`

Creates the `refresh_tokens` table with the following schema:

```sql
CREATE TABLE refresh_tokens (
    id              UUID        PRIMARY KEY,
    user_id         UUID        NOT NULL,
    token           VARCHAR(500) NOT NULL,
    expires_at_utc  TIMESTAMP   NOT NULL,
    revoked_at_utc  TIMESTAMP   NULL,
    created_at_utc  TIMESTAMP   NOT NULL,
    updated_at_utc  TIMESTAMP   NULL,
    is_deleted      BOOLEAN     NOT NULL DEFAULT FALSE
);

CREATE UNIQUE INDEX ON refresh_tokens (token);
CREATE INDEX ON refresh_tokens (user_id);
CREATE INDEX ON refresh_tokens (user_id, expires_at_utc);
```

---

## JWT Token Structure

A decoded access token looks like this:

```json
{
  "sub": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "alice@acme.com",
  "tenantId": "b1a5c9d2-0001-4f3e-9c1b-aabbccddeeff",
  "jti": "e8a7f2b1-3344-4abc-b123-000000000001",
  "role": ["Admin"],
  "iss": "dotnet-saas-api",
  "aud": "dotnet-saas-client",
  "iat": 1747354165,
  "exp": 1747355965
}
```

---

## Authentication Flow

```
Client                  API
  │                      │
  │── POST /auth/login ──▶│
  │                      │  Validate credentials
  │                      │  Hash(password) == storedHash?
  │                      │  Load user roles
  │                      │  GenerateAccessToken(user, roles)
  │                      │  GenerateRefreshToken()
  │                      │  Persist RefreshToken to DB
  │◀── 200 { accessToken, refreshToken } ──│
  │                      │
  │── GET /protected ────▶│  Authorization: Bearer <accessToken>
  │   [Bearer token]      │  UseAuthentication() validates token
  │                      │  ClaimsPrincipal populated
  │                      │  CurrentUserService.UserId available
  │◀── 200 OK ───────────│
  │                      │
  │── POST /auth/refresh ▶│  { refreshToken }
  │                      │  Load token from DB
  │                      │  Verify IsActive
  │                      │  Revoke old token
  │                      │  Issue new token pair
  │◀── 200 { accessToken, refreshToken } ──│
```

---

## Middleware Pipeline Order

```
Request
  │
  ▼
UseHttpsRedirection
  │
  ▼
UseTenantResolution        ← sets ITenantContext (Phase 5)
  │
  ▼
UseAuthentication          ← validates JWT, sets ClaimsPrincipal
  │
  ▼
UseAuthorization           ← enforces [Authorize] / policies
  │
  ▼
MapControllers
```

---

## Configuration

`appsettings.json` (development defaults — **replace SecretKey in production**):

```json
"Jwt": {
  "SecretKey": "your-secret-key-min-32-characters-long-change-in-production",
  "Issuer": "dotnet-saas-api",
  "Audience": "dotnet-saas-client",
  "AccessTokenExpirationMinutes": 30,
  "RefreshTokenExpirationDays": 7
}
```

In production, set `Jwt__SecretKey` as an environment variable or use a secret manager — never commit real keys to source control.

---

## Design Decisions

### Why is `IPasswordHashingService` in the Domain layer?

Domain services (e.g., a future `UserDomainService`) may need to validate passwords as part of business rules. Placing the interface in Domain avoids an upward dependency from Domain → Infrastructure.

### Why is `IRefreshTokenRepository` in the Application layer?

It follows the same pattern as all other repository interfaces in the project. The Application layer owns the contracts; Infrastructure owns the implementations. This keeps the Application layer testable without a real database.

### Why `BaseEntity` and not `AggregateRoot` for `RefreshToken`?

`RefreshToken` has no business significance beyond storing token state. It does not raise domain events and is always accessed through a `User` aggregate. Using `AggregateRoot` would be over-engineering and would add unnecessary domain event infrastructure to a simple persistence concern.

### Why `ClockSkew = TimeSpan.Zero`?

A non-zero clock skew silently extends token validity past the `exp` claim. In a multi-tenant API where tenant context is embedded in the token, any silent extension window is a security risk. Clients should handle token refresh proactively.

### Why store `tenantId` as a custom claim?

The standard JWT claim set has no `tenantId`. Using a custom claim (`"tenantId"`) makes the intent explicit and allows `CurrentUserService` to expose it as a first-class property, keeping tenant-aware code readable: `_currentUser.TenantId`.
