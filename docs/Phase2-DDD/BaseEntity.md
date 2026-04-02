# Base Entity

In Domain-Driven Design (DDD), a `BaseEntity` is a shared parent class for all entities in your domain.

It centralizes fields and behavior that are common across most entities, so each entity does not repeat the same boilerplate.

## Why Use BaseEntity

1. Consistent identity model.
2. Reusable audit metadata.
3. Shared lifecycle behavior (for example soft delete).
4. Cleaner entity classes focused on business rules.
5. Easier Infrastructure mapping (EF Core conventions/configuration).

## Typical Responsibilities

A `BaseEntity` usually contains:

- `Id`: unique identity for the entity.
- `CreatedAtUtc`: when it was created.
- `UpdatedAtUtc`: last update timestamp.
- `IsDeleted`: soft-delete flag.

It can also provide helper methods:

- `MarkUpdated()`
- `SoftDelete()`

## Example In This Project

```csharp
namespace Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? UpdatedAtUtc { get; protected set; }
    public bool IsDeleted { get; protected set; }

    protected BaseEntity(Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkUpdated() => UpdatedAtUtc = DateTime.UtcNow;

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
```

## How It Helps In Practice

Without `BaseEntity`, every entity (`User`, `Tenant`, `Role`, `Permission`, etc.) would need to repeat the same identity and audit fields.

With `BaseEntity`:

- all entities have the same base structure,
- interceptors can update audit fields in one generic way,
- global query filters can rely on `IsDeleted` consistently,
- your domain code stays easier to maintain.

## Entity vs Value Object

Do not use `BaseEntity` for value objects.

- Entity: has identity and lifecycle over time (use `BaseEntity`).
- Value Object: immutable and compared by value (inherits `ValueObject`, not `BaseEntity`).

## Why `Id` Is `Guid` Here

`Guid` is often used as the default entity key in distributed SaaS systems because it is easy to generate anywhere and avoids central ID coordination.

### Benefits of `Guid`

1. Globally unique across services, tenants, and environments.
2. Can be generated client-side or domain-side before database save.
3. Works well for offline workflows and eventual consistency.
4. Simple to pass through APIs and message events.
5. No database round-trip needed just to get an ID.

### Drawbacks of `Guid`

1. Larger storage and index size than `int`/`long`.
2. Random GUIDs can fragment clustered indexes.
3. Does not prevent mixing IDs of different entity types at compile time.

## Why Not Strongly Typed ID By Default

Strongly typed IDs (for example `UserId`, `TenantId`, `RoleId`) improve type safety, but they add complexity:

1. More code for value wrappers and conversions.
2. Extra EF Core mapping configuration.
3. More friction in serialization and tooling if not standardized.
4. Slightly steeper learning curve for new contributors.

Because of that, many teams start with `Guid` for entity primary keys and introduce strongly typed IDs where mistakes are expensive.

## Recommended Approach For This Project

Use a hybrid strategy:

1. Keep base entity key as `Guid` for simplicity and interoperability.
2. Use strongly typed IDs for critical boundaries (already done with `TenantId` value object).
3. Add more typed IDs later if you observe real bugs from ID mixing.

This gives a practical balance between delivery speed and domain safety.

## If You Want Full Strongly Typed IDs

Example of a typed user identity:

```csharp
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
```

Then entity key becomes:

```csharp
public UserId Id { get; protected set; }
```

EF Core needs conversion:

```csharp
builder.Property(x => x.Id)
    .HasConversion(id => id.Value, value => new UserId(value));
```

This is very safe, but adds mapping and plumbing overhead.

## Best Practices

1. Keep `BaseEntity` minimal. Add only truly common members.
2. Avoid putting business-specific logic in `BaseEntity`.
3. Use `protected set` for encapsulation.
4. Keep all timestamps in UTC.
5. Combine with `AggregateRoot` when entity needs domain events.

## Common Mistakes

1. Making `BaseEntity` too large (god base class).
2. Mixing persistence-specific concerns into domain logic.
3. Exposing public setters for identity/audit fields.
4. Treating soft delete as hard delete in business flows.

## Summary

`BaseEntity` gives your domain a consistent, reusable foundation for identity and lifecycle metadata.
It reduces duplication, improves maintainability, and supports patterns like auditing and soft delete across your whole model.

## Related Docs

- [AggregateRoot](./AggregateRoot.md)
- [ValueObject](./ValueObject.md)
- [Error](./Error.md)
