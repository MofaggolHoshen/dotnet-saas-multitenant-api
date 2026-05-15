# Phase 7 — Auth Feature: CQRS Implementation

**Phase**: 7  
**Status**: 🟢 Completed  
**Depends On**: Phase 6 (Authentication & JWT)  
**Duration**: 4 days

---

## Overview

Phase 7 wires together the JWT infrastructure from Phase 6 into a full set of Application-layer CQRS commands using MediatR. Every auth operation (login, register, refresh, logout, change password, forgot password, reset password) now has a dedicated `Command`, `Validator`, and `Handler` following the Clean Architecture conventions established in Phase 3.

---

## Files Added / Modified

| Action      | File                                                                                  | Description                                        |
| ----------- | ------------------------------------------------------------------------------------- | -------------------------------------------------- |
| ➕ Added    | `Application/Common/Interfaces/IJwtTokenGenerator.cs`                                 | Moved interface from Infrastructure to Application |
| ➕ Added    | `Application/Features/Auth/Commands/Login/LoginCommand.cs`                            | Command record + response DTO                      |
| ➕ Added    | `Application/Features/Auth/Commands/Login/LoginCommandValidator.cs`                   | Email + password validation                        |
| ➕ Added    | `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`                     | Full login flow with token generation              |
| ➕ Added    | `Application/Features/Auth/Commands/Register/RegisterCommand.cs`                      | Command record + response DTO                      |
| ➕ Added    | `Application/Features/Auth/Commands/Register/RegisterCommandValidator.cs`             | Email, password strength, confirm, full name       |
| ➕ Added    | `Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs`               | User creation + duplicate email check              |
| ➕ Added    | `Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommand.cs`              | Command record + response DTO                      |
| ➕ Added    | `Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommandValidator.cs`     | Token not-empty check                              |
| ➕ Added    | `Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs`       | Token rotation — revoke old, issue new pair        |
| ➕ Added    | `Application/Features/Auth/Commands/Logout/LogoutCommand.cs`                          | Command record                                     |
| ➕ Added    | `Application/Features/Auth/Commands/Logout/LogoutCommandValidator.cs`                 | Token not-empty check                              |
| ➕ Added    | `Application/Features/Auth/Commands/Logout/LogoutCommandHandler.cs`                   | Idempotent single-token revocation                 |
| ➕ Added    | `Application/Features/Auth/Commands/ChangePassword/ChangePasswordCommand.cs`          | Command record                                     |
| ➕ Added    | `Application/Features/Auth/Commands/ChangePassword/ChangePasswordCommandValidator.cs` | Old password, strength, confirm, differ-from-old   |
| ➕ Added    | `Application/Features/Auth/Commands/ChangePassword/ChangePasswordCommandHandler.cs`   | Verify old hash, update, revoke all tokens         |
| ➕ Added    | `Application/Features/Auth/Commands/ForgotPassword/ForgotPasswordCommand.cs`          | Command record (stub)                              |
| ➕ Added    | `Application/Features/Auth/Commands/ForgotPassword/ForgotPasswordCommandValidator.cs` | Email validation                                   |
| ➕ Added    | `Application/Features/Auth/Commands/ForgotPassword/ForgotPasswordCommandHandler.cs`   | Stub — always returns Success (anti-enumeration)   |
| ➕ Added    | `Application/Features/Auth/Commands/ResetPassword/ResetPasswordCommand.cs`            | Command record (stub)                              |
| ➕ Added    | `Application/Features/Auth/Commands/ResetPassword/ResetPasswordCommandValidator.cs`   | Token, password strength, confirm                  |
| ➕ Added    | `Application/Features/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs`     | Stub — returns not-implemented error               |
| 🔧 Modified | `Infrastructure/Identity/JwtTokenGenerator.cs`                                        | Removed inline `IJwtTokenGenerator` interface      |

---

## Architecture Decisions

### 1. `IJwtTokenGenerator` Moved to Application Layer

The interface was originally defined inside `Infrastructure.Identity.JwtTokenGenerator.cs` (Phase 6 convenience). Application handlers need to call it, but Application **cannot** reference Infrastructure (Clean Architecture dependency rule). The fix: move the interface to `Application.Common.Interfaces.IJwtTokenGenerator` and keep the concrete `JwtTokenGenerator` in Infrastructure implementing that interface.

**Before (violation):**

```
Application.Handler → using Infrastructure.Identity; (❌ circular-ish dependency)
```

**After (correct):**

```
Application.Handler → Application.Common.Interfaces.IJwtTokenGenerator (✅)
Infrastructure.JwtTokenGenerator → implements Application.Common.Interfaces.IJwtTokenGenerator (✅)
```

### 2. `ITenantContext` as Source of Tenant Identity

Commands like `Login`, `Register`, and `ForgotPassword` do **not** accept a `TenantId` parameter. The tenant is resolved from `ITenantContext` (set up by the middleware in Phase 5). This keeps the API surface clean and makes tenant leakage impossible.

### 3. Token Rotation (Refresh)

On every `RefreshToken` command, the old token is revoked before issuing a new access+refresh pair. This means a stolen refresh token can only be used once — the second use will be detected because the token is already revoked.

```
Client → POST /auth/refresh { refreshToken: "old" }
  → Load token, check IsActive
  → Revoke "old" token
  → Issue new access token + new refresh token
  → Return new pair
```

### 4. User Enumeration Prevention

Both `Login` and `ForgotPassword` return the **same generic error** regardless of whether the failure was due to a missing user, wrong password, or inactive account. This prevents attackers from probing which emails are registered.

| Operation      | Wrong email            | Wrong password         | Inactive user          |
| -------------- | ---------------------- | ---------------------- | ---------------------- |
| Login          | `Invalid credentials.` | `Invalid credentials.` | `Invalid credentials.` |
| ForgotPassword | `Result.Success()`     | —                      | —                      |

### 5. ChangePassword Revokes All Tokens

When a user changes their password, **all** active refresh tokens across all devices are revoked via `IRefreshTokenRepository.RevokeAllUserTokensAsync`. This forces re-login on all sessions — the secure default after a credential change.

### 6. ForgotPassword / ResetPassword are Stubs

Full email-based password reset requires an email notification service (future phase). The stubs:

- Accept and validate input correctly
- `ForgotPassword` always returns `Success` (anti-enumeration)
- `ResetPassword` returns a descriptive `not-implemented` error
- Both have full `TODO` comments explaining what the implementation needs

---

## Command Flow Diagrams

### Login Flow

```
POST /auth/login
  │
  ├─ LoginCommandValidator (FluentValidation via MediatR pipeline)
  │    ├─ Email: not empty + valid format
  │    └─ Password: not empty
  │
  └─ LoginCommandHandler
       ├─ ITenantContext.IsResolved? → else: Validation error
       ├─ TenantId.Create(tenantContext.TenantId)
       ├─ Email.Create(request.Email)
       ├─ IUserRepository.GetByEmailAsync(tenantId, email)
       ├─ IPasswordHashingService.Verify(password, hash)
       ├─ user.IsActive?
       │    └─ any failure → "Invalid credentials." (same error)
       ├─ IRoleRepository.GetByIdsAsync(user.RoleIds)
       ├─ IJwtTokenGenerator.GenerateAccessToken(user, roles)
       ├─ IJwtTokenGenerator.GenerateRefreshToken()
       ├─ RefreshToken.Create(userId, token, expiry)
       ├─ IRefreshTokenRepository.AddAsync(refreshToken)
       ├─ IUnitOfWork.SaveChangesAsync()
       └─ LoginResponse { AccessToken, RefreshToken, ExpiresAtUtc, UserId, Email, FullName }
```

### Register Flow

```
POST /auth/register
  │
  ├─ RegisterCommandValidator
  │    ├─ Email: not empty + valid format
  │    ├─ Password: not empty + PasswordValidator.IsValid()
  │    ├─ ConfirmPassword: equals Password
  │    └─ FullName: not empty, 2–100 chars
  │
  └─ RegisterCommandHandler
       ├─ ITenantContext.IsResolved? → else: Validation error
       ├─ TenantId.Create(tenantContext.TenantId)
       ├─ Email.Create(request.Email)
       ├─ IUserRepository.GetByEmailAsync() → if exists: Conflict error
       ├─ IPasswordHashingService.Hash(request.Password)
       ├─ User.Create(tenantId, email, hash, fullName)
       ├─ IUserRepository.AddAsync(user)
       ├─ IUnitOfWork.SaveChangesAsync()
       └─ RegisterResponse { UserId, Email, FullName, Message }
```

### RefreshToken Flow (Token Rotation)

```
POST /auth/refresh
  │
  ├─ RefreshTokenCommandValidator
  │    └─ RefreshToken: not empty
  │
  └─ RefreshTokenCommandHandler
       ├─ IRefreshTokenRepository.GetByTokenAsync(token)
       ├─ token.IsActive? → else: Conflict error (invalid/expired)
       ├─ IUserRepository.GetByIdAsync(token.UserId)
       ├─ user.IsActive? → else: Conflict error
       ├─ existingToken.Revoke()  ← token rotation
       ├─ IRefreshTokenRepository.UpdateAsync(existingToken)
       ├─ GetRoleNamesAsync(user.RoleIds)
       ├─ IJwtTokenGenerator.GenerateAccessToken(user, roles)
       ├─ IJwtTokenGenerator.GenerateRefreshToken()
       ├─ RefreshToken.Create(userId, newToken, expiry)
       ├─ IRefreshTokenRepository.AddAsync(newRefreshToken)
       ├─ IUnitOfWork.SaveChangesAsync()
       └─ RefreshTokenResponse { AccessToken, RefreshToken, ExpiresAtUtc }
```

---

## Validation Rules Summary

| Command        | Field              | Rules                                                                 |
| -------------- | ------------------ | --------------------------------------------------------------------- |
| Login          | Email              | NotEmpty, EmailAddress                                                |
| Login          | Password           | NotEmpty                                                              |
| Register       | Email              | NotEmpty, EmailAddress                                                |
| Register       | Password           | NotEmpty, MinLength(8), PasswordValidator (upper/lower/digit/special) |
| Register       | ConfirmPassword    | NotEmpty, Equal(Password)                                             |
| Register       | FullName           | NotEmpty, Length(2, 100)                                              |
| RefreshToken   | RefreshToken       | NotEmpty                                                              |
| Logout         | RefreshToken       | NotEmpty                                                              |
| ChangePassword | CurrentPassword    | NotEmpty                                                              |
| ChangePassword | NewPassword        | NotEmpty, MinLength(8), PasswordValidator, NotEqual(CurrentPassword)  |
| ChangePassword | ConfirmNewPassword | NotEmpty, Equal(NewPassword)                                          |
| ForgotPassword | Email              | NotEmpty, EmailAddress                                                |
| ResetPassword  | ResetToken         | NotEmpty                                                              |
| ResetPassword  | NewPassword        | NotEmpty, MinLength(8), PasswordValidator                             |
| ResetPassword  | ConfirmNewPassword | NotEmpty, Equal(NewPassword)                                          |

---

## Interfaces Used by Handlers

| Interface                 | Layer       | Purpose                               |
| ------------------------- | ----------- | ------------------------------------- |
| `ICommand<TResponse>`     | Application | MediatR command marker                |
| `ITenantContext`          | Application | Current tenant resolution             |
| `IUnitOfWork`             | Application | Persist changes atomically            |
| `ICurrentUserService`     | Application | Authenticated user's claims           |
| `IRefreshTokenRepository` | Application | CRUD + bulk-revoke for refresh tokens |
| `IUserRepository`         | Domain      | User lookup by ID / email             |
| `IRoleRepository`         | Domain      | Role names by ID list                 |
| `IPasswordHashingService` | Domain      | Hash + verify passwords               |
| `IJwtTokenGenerator`      | Application | Access + refresh token generation     |

---

## Key Notes for Future Phases

- **ForgotPassword / ResetPassword** are stubs. Completing them requires: a reset-token store (EF entity or Redis), an email service abstraction, and a token expiry policy.
- **The `ChangePassword` handler** calls `users.Update(user)` which tracks changes via EF Core's change tracker — no explicit SQL update needed.
- **Refresh token cleanup** (`IRefreshTokenRepository.DeleteExpiredTokensAsync`) is defined but not yet wired to a background job. Phase 16 (Redis + background jobs) is the natural place to add a Hangfire/IHostedService cleanup task.
- **`Domain.Entities.RefreshToken`** must be referenced with its full namespace inside handlers in the `Application.Features.Auth.Commands.RefreshToken` namespace to avoid ambiguity with the namespace name itself.
