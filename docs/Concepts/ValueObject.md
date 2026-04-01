# Value Object

In Domain-Driven Design (DDD), a **Value Object** is an object that is defined by its values, not by identity.

Two value objects are equal if all their important values are equal.

## Simple Explanation

A value object is about **what it contains**, not **who it is**.

- If two objects have the same value, they are treated as the same.
- It does not need its own unique `Id`.

Easy example:

- `Email("user@example.com")` and another `Email("user@example.com")` are considered equal.
- Because the value is the same, they represent the same concept.

Quick comparison:

- **Entity**: identity matters (`UserId` decides who it is).
- **Value Object**: value matters (`Email` text decides what it is).

## Why You Need Value Objects

Value objects solve common domain problems:

1. Prevent invalid data from entering your model.
2. Replace primitive types (`string`, `Guid`, `int`) with meaningful domain types.
3. Keep validation rules in one place.
4. Improve readability and type safety.
5. Make code easier to test and maintain.

Without value objects, business logic is spread across handlers/controllers/services and invalid state becomes easier to create.

## Entity vs Value Object

- **Entity**: has identity (`Id`) and lifecycle over time.
- **Value Object**: no identity, immutable, compared by value.

Example:

- `User` is an Entity.
- `Email`, `TenantId`, `SubscriptionTier` are Value Objects.

## Typical Characteristics

A good value object is:

1. Immutable after creation.
2. Self-validating at creation time.
3. Equality-based (by value).
4. Small and focused on a single concept.

## Base ValueObject Pattern

```csharp
namespace Domain.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate(0, HashCode.Combine);
    }
}
```

## Practical Example: Email Value Object

```csharp
using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex =
        new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<Email>.Failure(Error.Validation("Email is required."));
        }

        var normalized = input.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
        {
            return Result<Email>.Failure(Error.Validation("Email format is invalid."));
        }

        return Result<Email>.Success(new Email(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
```

## How This Helps in Real Code

Instead of this:

```csharp
public string Email { get; private set; }
```

Use this:

```csharp
public Email Email { get; private set; }
```

Now invalid email values cannot be assigned directly. Creation must go through `Email.Create(...)`.

## Key Benefits

1. **Stronger domain model**: business rules live with the data.
2. **Fewer bugs**: invalid values are rejected early.
3. **Less duplication**: validation is not repeated everywhere.
4. **Clearer intent**: `TenantId` is clearer than plain `Guid`.
5. **Better testability**: value object behavior can be unit-tested independently.

## Common Mistakes

1. Making value objects mutable.
2. Adding an `Id` field to value objects.
3. Skipping validation in factory methods.
4. Using exceptions for every validation case instead of result-based failures when appropriate.

## When Not to Use Value Objects

Do not create value objects for everything.

Use them when:

- The concept has clear domain meaning.
- Validation rules are important.
- Primitive obsession is causing confusion/bugs.

Avoid them for trivial fields with no behavior or business rule.

## Summary

A Value Object is one of the most useful DDD tools.
It gives you type safety, centralized validation, and clearer business code.

In this multi-tenant SaaS project, value objects like `Email`, `TenantId`, and `SubscriptionTier` help enforce domain rules consistently across the system.

## Related Docs

- [BaseEntity](./BaseEntity.md)
- [AggregateRoot](./AggregateRoot.md)
- [Error](./Error.md)
