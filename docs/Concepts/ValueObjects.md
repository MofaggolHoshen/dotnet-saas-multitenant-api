# Value Objects - Deep Dive

## 📖 Table of Contents
- [What are Value Objects?](#what-are-value-objects)
- [Value Objects vs Entities](#value-objects-vs-entities)
- [The Four Value Objects](#the-four-value-objects)
- [Implementation Patterns](#implementation-patterns)
- [Best Practices](#best-practices)

---

## What are Value Objects?

**Value Objects** are immutable objects defined by their **attributes**, not by identity. They:

- Have **no identity** (no ID property)
- Are **immutable** (cannot be changed after creation)
- Are **compared by value** (not by reference)
- **Validate themselves** on creation
- Provide **type safety** and domain meaning

### Why Use Value Objects?

```mermaid
graph TB
    subgraph "Without Value Objects ❌"
        P1[Primitive Obsession]
        P1 --> P2[string email<br/>No validation]
        P1 --> P3[string password<br/>No rules]
        P1 --> P4[Guid tenantId<br/>Can be empty]
    end

    subgraph "With Value Objects ✅"
        V1[Rich Domain Types]
        V1 --> V2[Email<br/>Validated format]
        V1 --> V3[Password<br/>Enforced strength]
        V1 --> V4[TenantId<br/>Non-empty guaranteed]
    end

    style P1 fill:#ffe1e1
    style V1 fill:#e1ffe1
```

**Benefits:**
- 🛡️ **Type Safety:** Can't pass email where tenantId is expected
- ✅ **Self-Validation:** Value object guarantees its own validity
- 📦 **Encapsulation:** Business rules in one place
- 🔄 **Reusability:** Use across multiple entities
- 📖 **Expressiveness:** `Email` is clearer than `string`

---

## Value Objects vs Entities

| Aspect | Value Object | Entity |
|--------|-------------|---------|
| **Identity** | No unique ID | Has unique ID |
| **Equality** | By value (all properties) | By ID |
| **Mutability** | Immutable | Mutable |
| **Lifecycle** | Created and discarded | Tracked over time |
| **Example** | Email, Money, Address | User, Order, Product |

### Equality Comparison

```mermaid
sequenceDiagram
    participant Code
    participant Email1
    participant Email2
    participant User1
    participant User2

    Note over Code: Value Object Equality
    Code->>Email1: new Email("test@example.com")
    Code->>Email2: new Email("test@example.com")
    Code->>Email1: Equals(Email2)?
    Email1-->>Code: TRUE (same value)

    Note over Code: Entity Equality
    Code->>User1: new User(Id: 123)
    Code->>User2: new User(Id: 456)
    Code->>User1: Equals(User2)?
    User1-->>Code: FALSE (different IDs)

    Code->>User1: Change Email to new value
    Code->>User1: Equals(original User1)?
    User1-->>Code: TRUE (same ID)
```

---

## The Four Value Objects

### 1. 📧 Email Value Object

**Purpose:** Ensure valid email format and provide normalization.

```csharp
public sealed class Email : ValueObject
{
    public const int MaxLength = 256;

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure(Error.Validation("Email is required."));

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
            return Result<Email>.Failure(
                Error.Validation($"Email must not exceed {MaxLength} characters."));

        if (!IsValidFormat(email))
            return Result<Email>.Failure(Error.Validation("Email format is invalid."));

        return Result<Email>.Success(new Email(email));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Visual Flow:**

```mermaid
flowchart TD
    Start([User Input]) --> Input["  john@EXAMPLE.com  "]
    Input --> Validate{Validate}

    Validate -->|Empty| E1[Error: Required]
    Validate -->|Too Long| E2[Error: Max 256 chars]
    Validate -->|Invalid Format| E3[Error: Invalid format]
    Validate -->|Valid| Normalize[Normalize]

    Normalize --> Trim[Trim whitespace]
    Trim --> Lower[ToLowerInvariant]
    Lower --> Create[Create Email VO]
    Create --> Success([john@example.com ✅])

    style Start fill:#e1f5ff
    style Success fill:#e1ffe1
    style E1 fill:#ffe1e1
    style E2 fill:#ffe1e1
    style E3 fill:#ffe1e1
```

**Usage Example:**

```csharp
// ✅ Type-safe and validated
var emailResult = Email.Create("user@example.com");
if (emailResult.IsFailure)
{
    return emailResult.Error;
}

var user = User.Create(tenantId, emailResult.Value, hash, "John");

// ❌ Cannot do this (compile error):
var user = User.Create(tenantId, "not-validated-string", hash, "John");
```

---

### 2. 🔒 Password Value Object

**Purpose:** Enforce password strength requirements.

```csharp
public sealed class Password : ValueObject
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    private Password(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Password> Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<Password>.Failure(Error.Validation("Password is required."));

        if (password.Length < MinLength)
            return Result<Password>.Failure(
                Error.Validation($"Password must be at least {MinLength} characters."));

        if (password.Length > MaxLength)
            return Result<Password>.Failure(
                Error.Validation($"Password must not exceed {MaxLength} characters."));

        if (!HasRequiredComplexity(password))
            return Result<Password>.Failure(
                Error.Validation("Password must contain uppercase, lowercase, digit, and special character."));

        return Result<Password>.Success(new Password(password));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Password Validation Flow:**

```mermaid
flowchart TD
    Start([User Input]) --> Pass[Enter Password]
    Pass --> Check1{Length >= 8?}

    Check1 -->|No| E1[❌ Too short]
    Check1 -->|Yes| Check2{Has Uppercase?}

    Check2 -->|No| E2[❌ Need uppercase]
    Check2 -->|Yes| Check3{Has Lowercase?}

    Check3 -->|No| E3[❌ Need lowercase]
    Check3 -->|Yes| Check4{Has Digit?}

    Check4 -->|No| E4[❌ Need digit]
    Check4 -->|Yes| Check5{Has Special Char?}

    Check5 -->|No| E5[❌ Need special char]
    Check5 -->|Yes| Hash[Hash Password]

    Hash --> Create[Create Password VO]
    Create --> Success([✅ Valid Password])

    style Start fill:#e1f5ff
    style Success fill:#e1ffe1
    style E1 fill:#ffe1e1
    style E2 fill:#ffe1e1
    style E3 fill:#ffe1e1
    style E4 fill:#ffe1e1
    style E5 fill:#ffe1e1
```

**Security Pattern:**

```mermaid
sequenceDiagram
    participant User
    participant PasswordVO
    participant HashService
    participant UserEntity

    User->>PasswordVO: Create("MyPassword123!")
    PasswordVO->>PasswordVO: Validate complexity
    alt Valid
        PasswordVO-->>User: Result.Success(Password)
        User->>HashService: Hash(password.Value)
        HashService-->>User: passwordHash
        User->>UserEntity: Create(..., passwordHash, ...)
    else Invalid
        PasswordVO-->>User: Result.Failure(Error)
    end
```

---

### 3. 🏢 TenantId Value Object

**Purpose:** Strong typing for tenant identifiers to prevent bugs.

```csharp
public sealed class TenantId : ValueObject
{
    private TenantId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Result<TenantId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result<TenantId>.Failure(Error.Validation("TenantId cannot be empty."));

        return Result<TenantId>.Success(new TenantId(value));
    }

    public static TenantId CreateUnsafe(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Type Safety Example:**

```csharp
// ❌ Without TenantId (Primitive Obsession)
public class User
{
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
}

// Bug: Can accidentally swap IDs
var user = new User 
{ 
    TenantId = roleGuid,  // WRONG! But compiles
    RoleId = tenantGuid   // WRONG! But compiles
};

// ✅ With TenantId (Type Safety)
public class User
{
    public TenantId TenantId { get; private set; }
    public Guid RoleId { get; private set; }
}

// Compile error: Cannot assign Guid to TenantId
var user = User.Create(roleGuid, ...); // ❌ Compile Error!
var user = User.Create(tenantId, ...);  // ✅ Correct
```

**Multi-Tenancy Isolation:**

```mermaid
graph TB
    subgraph "Tenant Isolation"
        T1[TenantId: A]
        T2[TenantId: B]

        U1[User 1] --> T1
        U2[User 2] --> T1
        U3[User 3] --> T2
        U4[User 4] --> T2

        R1[Role X] --> T1
        R2[Role Y] --> T2
    end

    Query[Query: GetUsers<br/>TenantId: A] --> Filter{Filter by TenantId}
    Filter -->|Match| U1
    Filter -->|Match| U2
    Filter -.Exclude.-> U3
    Filter -.Exclude.-> U4

    style T1 fill:#e1f5ff
    style T2 fill:#f0e1ff
    style Query fill:#fff4e1
```

---

### 4. 💳 SubscriptionTier Value Object

**Purpose:** Enum-based value object for subscription plans.

```csharp
public sealed class SubscriptionTier : ValueObject
{
    public static readonly SubscriptionTier Free = new(0, "Free", 1, 5);
    public static readonly SubscriptionTier Basic = new(1, "Basic", 10, 25);
    public static readonly SubscriptionTier Premium = new(2, "Premium", 50, 100);
    public static readonly SubscriptionTier Enterprise = new(3, "Enterprise", int.MaxValue, int.MaxValue);

    private SubscriptionTier(int value, string name, int maxUsers, int maxRoles)
    {
        Value = value;
        Name = name;
        MaxUsers = maxUsers;
        MaxRoles = maxRoles;
    }

    public int Value { get; }
    public string Name { get; }
    public int MaxUsers { get; }
    public int MaxRoles { get; }

    public static SubscriptionTier FromValue(int value) => value switch
    {
        0 => Free,
        1 => Basic,
        2 => Premium,
        3 => Enterprise,
        _ => throw new ArgumentException($"Invalid subscription tier: {value}")
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Subscription Comparison:**

```mermaid
graph LR
    subgraph "Subscription Tiers"
        F[🆓 Free<br/>1 user<br/>5 roles]
        B[💼 Basic<br/>10 users<br/>25 roles]
        P[⭐ Premium<br/>50 users<br/>100 roles]
        E[🏢 Enterprise<br/>Unlimited<br/>Unlimited]
    end

    F -.upgrade.-> B
    B -.upgrade.-> P
    P -.upgrade.-> E

    style F fill:#e1f5ff
    style B fill:#e1ffe1
    style P fill:#fff4e1
    style E fill:#ffd700
```

**Usage with Business Logic:**

```csharp
public Result AddUser(User newUser)
{
    var currentUserCount = _users.Count;

    if (currentUserCount >= Tier.MaxUsers)
    {
        return Result.Failure(
            Error.Conflict(
                $"Cannot add user. Tier '{Tier.Name}' allows max {Tier.MaxUsers} users."));
    }

    _users.Add(newUser);
    return Result.Success();
}
```

---

## Implementation Patterns

### 1. Base ValueObject Class

```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    // Subclasses define their equality components
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + (obj?.GetHashCode() ?? 0);
                }
            });
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left is null && right is null || left is not null && left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}
```

### 2. Validation Pattern

```mermaid
flowchart TD
    Start([Create Value Object]) --> Factory[Static Factory Method]
    Factory --> V1{Null Check}

    V1 -->|Fail| E1[Return Error]
    V1 -->|Pass| V2{Length Check}

    V2 -->|Fail| E2[Return Error]
    V2 -->|Pass| V3{Format Check}

    V3 -->|Fail| E3[Return Error]
    V3 -->|Pass| V4{Business Rules}

    V4 -->|Fail| E4[Return Error]
    V4 -->|Pass| Create[Create Instance]

    Create --> Wrap[Wrap in Result.Success]
    Wrap --> End([Return Result])

    E1 --> End
    E2 --> End
    E3 --> End
    E4 --> End

    style Start fill:#e1f5ff
    style End fill:#e1ffe1
    style E1 fill:#ffe1e1
    style E2 fill:#ffe1e1
    style E3 fill:#ffe1e1
    style E4 fill:#ffe1e1
```

### 3. Immutability Pattern

```csharp
// ✅ IMMUTABLE - Value Object
public sealed class Email : ValueObject
{
    private Email(string value)  // Private constructor
    {
        Value = value;
    }

    public string Value { get; }  // Read-only property

    // No setters, no mutating methods
    // To change email, create new instance
}

// Usage
var email1 = Email.Create("old@example.com").Value;
var email2 = Email.Create("new@example.com").Value;  // New instance

// ❌ Cannot do this (immutable):
email1.Value = "changed";  // Compile error!
```

---

## Best Practices

### ✅ DO

1. **Always Validate in Factory Methods**
   ```csharp
   public static Result<Email> Create(string email)
   {
       if (string.IsNullOrWhiteSpace(email))
           return Result<Email>.Failure(Error.Validation("Email is required."));

       // More validation...
       return Result<Email>.Success(new Email(email));
   }
   ```

2. **Make Constructors Private**
   ```csharp
   private Email(string value)  // Forces use of factory method
   {
       Value = value;
   }
   ```

3. **Use Read-Only Properties**
   ```csharp
   public string Value { get; }  // No setter
   ```

4. **Implement Equality by Value**
   ```csharp
   protected override IEnumerable<object?> GetEqualityComponents()
   {
       yield return Value;
   }
   ```

5. **Normalize Input**
   ```csharp
   email = email.Trim().ToLowerInvariant();
   ```

### ❌ DON'T

1. **Don't Use Public Constructors**
   ```csharp
   // ❌ Allows invalid creation
   public Email(string value) { Value = value; }
   ```

2. **Don't Add Setters**
   ```csharp
   // ❌ Breaks immutability
   public string Value { get; set; }
   ```

3. **Don't Skip Validation**
   ```csharp
   // ❌ No validation
   public static Email Create(string email) => new Email(email);
   ```

4. **Don't Add Identity**
   ```csharp
   // ❌ Value objects don't have IDs
   public Guid Id { get; set; }
   ```

5. **Don't Add Mutating Methods**
   ```csharp
   // ❌ Violates immutability
   public void ChangeValue(string newValue) { Value = newValue; }
   ```

---

## Comparison Matrix

| Feature | Email | Password | TenantId | SubscriptionTier |
|---------|-------|----------|----------|------------------|
| **Validation** | Format, Length | Complexity, Length | Non-empty GUID | Valid enum value |
| **Normalization** | Trim, Lowercase | None | None | None |
| **Max Length** | 256 chars | 128 chars | N/A | N/A |
| **Min Length** | 1 char | 8 chars | N/A | N/A |
| **Type** | String-based | String-based | GUID-based | Enum-based |
| **Usage** | User identity | Authentication | Isolation | Features |

---

## Summary

Value Objects provide:

- 🛡️ **Type Safety** - Compile-time guarantees
- ✅ **Self-Validation** - Always valid
- 🔒 **Immutability** - Thread-safe by design
- 📦 **Encapsulation** - Rules in one place
- 🔄 **Reusability** - Used across entities
- 📖 **Expressiveness** - Domain language

```mermaid
mindmap
  root((Value Objects))
    Type Safety
      No primitive obsession
      Compile-time checks
      Clear intent
    Validation
      Self-validating
      Fail-fast
      Consistent rules
    Immutability
      Thread-safe
      Predictable
      No side effects
    Reusability
      DRY principle
      Single source of truth
      Shared across entities
```

---

**Next:** Learn about [Domain Events](./DomainEvents.md) for capturing state changes.

**Last Updated:** April 02, 2026
