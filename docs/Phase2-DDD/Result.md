# Result Pattern

In DDD/Clean Architecture, a `Result` object is used to represent the outcome of an operation without always throwing exceptions.

It usually has two states:

- Success
- Failure (with an `Error`)

## Simple Explanation

Think of `Result` as a safe response wrapper.

Instead of crashing with exceptions for normal business failures, you return a clear outcome:

- `Success` when operation is valid
- `Failure` when business rule fails

Example:

- Login with wrong password is not a system crash.
- It should return `Failure("Invalid credentials")`.

## Why You Need Result

### 1. Handle expected failures cleanly
Many failures are normal in business logic:

- Email format invalid
- User already exists
- User is inactive
- Tenant not found

These should not be treated as app crashes.

### 2. Reduce exception overuse
Using exceptions for every invalid input creates noisy code and harder control flow.

`Result` keeps business flow explicit and predictable.

### 3. Better API response mapping
Controller can map failures to HTTP responses consistently.

- `Validation` -> `400 BadRequest`
- `NotFound` -> `404 NotFound`
- `Conflict` -> `409 Conflict`

### 4. Easier testing
In tests, you can directly assert:

- `result.IsSuccess`
- `result.IsFailure`
- `result.Error.Code`

No need to test many thrown exceptions for common cases.

## Typical Shape

```csharp
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Successful result cannot contain error.");

        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Failed result must contain error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result.");

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(Error error) => new(error);
}
```

## Practical Domain Example

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

## Practical Handler Example

```csharp
public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
{
    var emailResult = Email.Create(request.Email);
    if (emailResult.IsFailure)
    {
        return Result<Guid>.Failure(emailResult.Error);
    }

    var existing = await _userRepository.GetByEmailAsync(request.TenantId, emailResult.Value.Value, ct);
    if (existing is not null)
    {
        return Result<Guid>.Failure(Error.Conflict("User with this email already exists."));
    }

    // Save user...
    return Result<Guid>.Success(user.Id);
}
```

## Benefits Summary

1. Predictable business flow.
2. Clear separation between business failures and technical exceptions.
3. Cleaner and more maintainable handlers.
4. Consistent API error responses.
5. Better unit test readability.

## Result vs Exception

Use `Result` when:

- Failure is expected and part of business rules.

Use `Exception` when:

- Failure is unexpected (DB/network/runtime issues).

## Quick Guideline

1. Domain validation fails -> return `Result.Failure(Error...)`
2. Unexpected technical issue -> throw exception
3. In API layer, map `Result` failures to HTTP status codes

## Related Docs

- [Error](./Error.md)
- [ValueObject](./ValueObject.md)
- [BaseEntity](./BaseEntity.md)
- [AggregateRoot](./AggregateRoot.md)
