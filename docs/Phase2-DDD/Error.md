# Error Concept (DDD + Result Pattern)

The `Error` class is used to represent expected failures in a structured way.

It is not a replacement for exceptions in unexpected crashes. Instead, it is ideal for business/domain failures that can happen during normal application flow.

## What Is `Error`

In simple terms, `Error` carries two things:

- `Code` (machine-friendly): `Validation`, `NotFound`, `Conflict`
- `Message` (human-friendly): clear explanation of what failed

Example:

```csharp
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error Validation(string message) => new("Validation", message);
    public static Error NotFound(string message) => new("NotFound", message);
    public static Error Conflict(string message) => new("Conflict", message);
}
```

## Why You Need It

### 1. Handle expected failures cleanly
Many failures are normal business outcomes:

- Invalid email format
- User already exists
- Tenant not found
- Invalid role assignment

These are not system crashes, so returning `Result.Failure(Error...)` is usually better than throwing exceptions.

### 2. Keep API responses consistent
All handlers can return the same failure shape:

- `Code`
- `Message`

Then controllers/middleware can map errors to HTTP status codes consistently.

### 3. Improve testability
In unit tests, you can assert:

- Success/failure state
- Exact `Error.Code`
- Exact `Error.Message`

### 4. Make business intent explicit
`Error.Conflict("Cannot assign role to inactive user")` immediately communicates a domain rule violation.

## `Error` vs `Exception` (When to use each)

Use `Error` when the failure is expected and recoverable:

- Validation failed
- Entity not found
- Business rule violation

Use `Exception` when the failure is unexpected and technical:

- Database connection failure
- Null reference bug
- Serialization/runtime crash

## Real Example

Domain method:

```csharp
public Result AssignRole(Guid roleId)
{
    if (!IsActive)
    {
        return Result.Failure(Error.Conflict("Cannot assign role to inactive user."));
    }

    if (roleId == Guid.Empty)
    {
        return Result.Failure(Error.Validation("RoleId cannot be empty."));
    }

    return Result.Success();
}
```

Controller mapping:

```csharp
if (result.IsFailure)
{
    return result.Error.Code switch
    {
        "NotFound" => NotFound(result.Error.Message),
        "Conflict" => Conflict(result.Error.Message),
        _ => BadRequest(result.Error.Message)
    };
}
```

## Benefits Summary

1. Predictable failure handling.
2. Cleaner code with less unnecessary try/catch for business rules.
3. Consistent API error responses.
4. Easier unit testing.
5. Clear separation between business failures and technical faults.

## Quick Guideline

1. Business rule failure -> `Result.Failure(Error...)`
2. Unexpected technical fault -> throw exception
3. In API layer, map `Error.Code` to the correct HTTP status

## Final Note

In a multi-tenant SaaS system, predictable error handling is critical because validation, authorization, and tenant boundaries are checked frequently.

Using the `Error + Result` pattern makes the codebase more robust, readable, and maintainable.

## Related Docs

- [BaseEntity](./BaseEntity.md)
- [AggregateRoot](./AggregateRoot.md)
- [ValueObject](./ValueObject.md)
