# Concrete Example: Create User - Complete Data Flow

> **Step-by-step code walkthrough showing how Domain Events (IDomainEvent) flow through the entire application**

This document shows the **ACTUAL CODE** that executes when creating a user, from the HTTP request to the database and event handlers.

---

## 🎯 The Complete Flow (High-Level)

```
Client Request 
    ↓
API Controller 
    ↓
MediatR Command 
    ↓
Command Handler 
    ↓
Domain Entity (User.Create) → 🔔 Raises UserCreatedEvent (IDomainEvent)
    ↓
Repository.AddAsync() 
    ↓
UnitOfWork.SaveChangesAsync() → 📢 Dispatches Domain Events
    ↓
Event Handlers Execute (Send Email, Log Audit, etc.)
    ↓
Response to Client
```

---

## 📝 Step-by-Step Code Execution

### **STEP 1: Client Sends HTTP Request**

```http
POST https://api.yourapp.com/api/users
Content-Type: application/json
Authorization: Bearer {jwt-token}

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe"
}
```

---

### **STEP 2: API Controller Receives Request**

**File:** `source/WebAPI/Controllers/UsersController.cs` (or Minimal API)

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(
    [FromBody] CreateUserRequest request,
    [FromServices] IMediator mediator)
{
    // 1. Create the command
    var command = new CreateUserCommand(
        request.Email,
        request.Password,
        request.FullName
    );

    // 2. Send command to MediatR
    var result = await mediator.Send(command);

    // 3. Return response
    return result.IsSuccess
        ? CreatedAtAction(nameof(GetUserById), new { id = result.Value.Id }, result.Value)
        : BadRequest(result.Error);
}
```

---

### **STEP 3: MediatR Routes to Command Handler**

**File:** `source/Application/Features/Users/Commands/CreateUserCommandHandler.cs`

```csharp
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Users.Commands;

// The Command
public record CreateUserCommand(
    string Email,
    string Password,
    string FullName
) : IRequest<Result<UserDto>>;

// The Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordService;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHashingService passwordService,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // ────────────────────────────────────────────────────
        // STEP 3.1: Create Value Objects
        // ────────────────────────────────────────────────────

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result<UserDto>.Failure(emailResult.Error);
        }

        var tenantIdResult = TenantId.Create(_tenantContext.CurrentTenantId);
        if (tenantIdResult.IsFailure)
        {
            return Result<UserDto>.Failure(tenantIdResult.Error);
        }

        // ────────────────────────────────────────────────────
        // STEP 3.2: Hash Password
        // ────────────────────────────────────────────────────

        var passwordHash = _passwordService.HashPassword(request.Password);

        // ────────────────────────────────────────────────────
        // STEP 3.3: Check if User Already Exists
        // ────────────────────────────────────────────────────

        var existingUser = await _userRepository.GetByEmailAsync(
            emailResult.Value,
            tenantIdResult.Value,
            cancellationToken);

        if (existingUser is not null)
        {
            return Result<UserDto>.Failure(
                Error.Conflict("User", "A user with this email already exists."));
        }

        // ────────────────────────────────────────────────────
        // STEP 3.4: 🎯 CREATE USER ENTITY (Domain Event Raised Here!)
        // ────────────────────────────────────────────────────

        var userResult = User.Create(
            tenantIdResult.Value,
            emailResult.Value,
            passwordHash,
            request.FullName);

        if (userResult.IsFailure)
        {
            return Result<UserDto>.Failure(userResult.Error);
        }

        var user = userResult.Value;

        // ⚠️ AT THIS POINT: user._domainEvents contains UserCreatedEvent!
        // The event is stored in memory, NOT dispatched yet

        // ────────────────────────────────────────────────────
        // STEP 3.5: Add User to Repository (in-memory tracking)
        // ────────────────────────────────────────────────────

        await _userRepository.AddAsync(user, cancellationToken);

        // ────────────────────────────────────────────────────
        // STEP 3.6: 💾 SAVE TO DATABASE (Events dispatched here!)
        // ────────────────────────────────────────────────────

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ After SaveChangesAsync:
        // - User is saved to database
        // - Domain events are dispatched
        // - Event handlers execute

        // ────────────────────────────────────────────────────
        // STEP 3.7: Map to DTO and Return
        // ────────────────────────────────────────────────────

        var userDto = new UserDto(
            user.Id,
            user.Email.Value,
            user.FullName,
            user.IsActive,
            user.TenantId.Value
        );

        return Result<UserDto>.Success(userDto);
    }
}
```

---

### **STEP 4: 🔔 Domain Entity Raises Event**

**File:** `source/Domain/Entities/User.cs`

```csharp
public static Result<User> Create(
    TenantId tenantId, 
    Email email, 
    string passwordHash, 
    string fullName)
{
    // Validation
    if (string.IsNullOrWhiteSpace(passwordHash))
    {
        return Result<User>.Failure(Error.Validation("Password hash is required."));
    }

    if (string.IsNullOrWhiteSpace(fullName))
    {
        return Result<User>.Failure(Error.Validation("Full name is required."));
    }

    // ────────────────────────────────────────────────────
    // 🎯 Create the User
    // ────────────────────────────────────────────────────
    var user = new User(tenantId, email, passwordHash, fullName.Trim());

    // ────────────────────────────────────────────────────
    // 🔔 RAISE DOMAIN EVENT (stored in _domainEvents list)
    // ────────────────────────────────────────────────────
    user.AddDomainEvent(new UserCreatedEvent(
        user.Id,           // Guid UserId
        user.TenantId.Value, // Guid TenantId
        user.Email.Value    // string Email
    ));

    // ⚠️ The event is NOT dispatched yet!
    // It's stored in the AggregateRoot._domainEvents list

    return Result<User>.Success(user);
}
```

**What `AddDomainEvent` does:**

**File:** `source/Domain/Common/AggregateRoot.cs`

```csharp
public abstract class AggregateRoot : BaseEntity
{
    // 📦 In-memory storage for events
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        // ✅ Add event to the list (in-memory, not dispatched)
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

---

### **STEP 5: The Event Object**

**File:** `source/Domain/Events/UserCreatedEvent.cs`

```csharp
namespace Domain.Events;

// ✅ This is the actual event object
public sealed record UserCreatedEvent(
    Guid UserId,      // The created user's ID
    Guid TenantId,    // The tenant ID
    string Email      // The user's email
) : IDomainEvent     // ← Implements IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
```

**File:** `source/Domain/Events/IDomainEvent.cs`

```csharp
namespace Domain.Events;

// ✅ Marker interface for all domain events
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
```

**Why use a marker interface?**
- MediatR uses this to identify domain events
- Enables polymorphic handling
- Provides a common contract

---

### **STEP 6: Repository Adds User (In-Memory Tracking)**

**File:** `source/Infrastructure/Persistence/Repositories/UserRepository.cs`

```csharp
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        // ✅ EF Core tracks the entity in memory
        // The domain events are still in user._domainEvents
        await _context.Users.AddAsync(user, ct);

        // ⚠️ NOT saved to database yet!
        // ⚠️ Events NOT dispatched yet!
    }

    // Other methods...
}
```

---

### **STEP 7: 💾📢 UnitOfWork Saves AND Dispatches Events**

**File:** `source/Infrastructure/Persistence/UnitOfWork.cs`

```csharp
using Domain.Events;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;

    public UnitOfWork(ApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // ────────────────────────────────────────────────────
        // 🔍 STEP 7.1: Get all domain events BEFORE saving
        // ────────────────────────────────────────────────────

        var domainEvents = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        // At this point, domainEvents list contains:
        // - UserCreatedEvent { UserId, TenantId, Email, OccurredOnUtc }

        // ────────────────────────────────────────────────────
        // 💾 STEP 7.2: Save changes to database
        // ────────────────────────────────────────────────────

        var result = await _context.SaveChangesAsync(ct);

        // ✅ User is now persisted in the database
        // ✅ Transaction committed

        // ────────────────────────────────────────────────────
        // 🧹 STEP 7.3: Clear events from aggregates
        // ────────────────────────────────────────────────────

        var aggregates = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        // ────────────────────────────────────────────────────
        // 📢 STEP 7.4: Dispatch events to handlers
        // ────────────────────────────────────────────────────

        foreach (var domainEvent in domainEvents)
        {
            // ✅ THIS IS WHERE EVENTS ARE DISPATCHED!
            await _mediator.Publish(domainEvent, ct);

            // MediatR will find all INotificationHandler<UserCreatedEvent>
            // and execute them (in parallel by default)
        }

        return result;
    }

    // Transaction methods...
}
```

**Key Points:**
1. **Before SaveChangesAsync:** Events stored in memory
2. **During SaveChangesAsync:** Database write happens
3. **After SaveChangesAsync:** Events are published to handlers
4. **Why this order?** Events only dispatch if database save succeeds

---

### **STEP 8: 🎯 Event Handlers Execute**

**File:** `source/Application/Features/Users/EventHandlers/UserCreatedEventHandler.cs`

```csharp
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.EventHandlers;

// ✅ This class is discovered by MediatR at startup
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IEmailService emailService,
        ILogger<UserCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    // ✅ This method is called when UserCreatedEvent is published
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "📧 Handling UserCreatedEvent for User {UserId} in Tenant {TenantId}",
            notification.UserId,
            notification.TenantId);

        // ────────────────────────────────────────────────────
        // 📨 SEND WELCOME EMAIL
        // ────────────────────────────────────────────────────

        try
        {
            await _emailService.SendWelcomeEmailAsync(
                notification.Email,
                cancellationToken);

            _logger.LogInformation(
                "✅ Welcome email sent to {Email}",
                notification.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to send welcome email to {Email}",
                notification.Email);

            // Note: We don't throw - other handlers should still execute
        }
    }
}
```

**File:** `source/Application/Features/Users/EventHandlers/UserAuditEventHandler.cs`

```csharp
using Domain.Events;
using MediatR;

namespace Application.Features.Users.EventHandlers;

// ✅ Multiple handlers can listen to the same event
public class UserAuditEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IAuditService _auditService;

    public UserAuditEventHandler(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ────────────────────────────────────────────────────
        // 📝 LOG AUDIT TRAIL
        // ────────────────────────────────────────────────────

        await _auditService.LogEventAsync(
            eventType: "UserCreated",
            userId: notification.UserId,
            tenantId: notification.TenantId,
            details: $"User {notification.Email} created",
            occurredAt: notification.OccurredOnUtc,
            cancellationToken);
    }
}
```

**File:** `source/Application/Features/Users/EventHandlers/UserCacheEventHandler.cs`

```csharp
using Domain.Events;
using MediatR;

namespace Application.Features.Users.EventHandlers;

public class UserCacheEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ICacheService _cacheService;

    public UserCacheEventHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ────────────────────────────────────────────────────
        // 🗑️ INVALIDATE CACHE
        // ────────────────────────────────────────────────────

        var cacheKey = $"users:tenant:{notification.TenantId}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
    }
}
```

**All three handlers execute in parallel!**

---

### **STEP 9: Handler Registration at Startup**

**File:** `source/WebAPI/Program.cs`

```csharp
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────────────────
// 🔧 Register MediatR (scans for handlers)
// ────────────────────────────────────────────────────

builder.Services.AddMediatR(cfg =>
{
    // Scan Application assembly for:
    // - IRequestHandler<TCommand, TResponse>
    // - INotificationHandler<TEvent>
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
});

// MediatR finds:
// ✅ UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
// ✅ UserAuditEventHandler : INotificationHandler<UserCreatedEvent>
// ✅ UserCacheEventHandler : INotificationHandler<UserCreatedEvent>

// ────────────────────────────────────────────────────
// 🔧 Register Infrastructure
// ────────────────────────────────────────────────────

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ────────────────────────────────────────────────────
// 🔧 Register Services (used by handlers)
// ────────────────────────────────────────────────────

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

var app = builder.Build();
app.Run();
```

---

## 🔍 Visual Timeline

```
Time  │ Layer              │ Action
──────┼────────────────────┼─────────────────────────────────────────
0ms   │ API Controller     │ Receive HTTP POST /api/users
5ms   │ MediatR            │ Route to CreateUserCommandHandler
10ms  │ Command Handler    │ Validate email, hash password
15ms  │ Domain Entity      │ User.Create() called
      │                    │   ├─ Create user object
      │                    │   └─ 🔔 AddDomainEvent(UserCreatedEvent)
      │                    │      └─ Event stored in _domainEvents
20ms  │ Repository         │ userRepository.AddAsync(user)
      │                    │   └─ EF Core tracks entity (in-memory)
25ms  │ UnitOfWork         │ unitOfWork.SaveChangesAsync() called
      │                    │   ├─ Collect domain events from aggregates
30ms  │ Database           │   ├─ 💾 INSERT INTO Users (...)
35ms  │ Database           │   └─ ✅ COMMIT TRANSACTION
40ms  │ UnitOfWork         │   ├─ Clear events from aggregates
      │                    │   └─ 📢 Dispatch events
      │                    │      └─ _mediator.Publish(UserCreatedEvent)
45ms  │ MediatR            │ Resolve all INotificationHandler<UserCreatedEvent>
      │                    │   ├─ UserCreatedEventHandler
      │                    │   ├─ UserAuditEventHandler
      │                    │   └─ UserCacheEventHandler
      │                    │
      │ ╔══════════════════ PARALLEL EXECUTION ═══════════════╗
50ms  │ Handler 1          │ │ 📧 Send welcome email            │
60ms  │ Handler 2          │ │ 📝 Log audit trail               │
55ms  │ Handler 3          │ │ 🗑️ Invalidate user cache        │
      │ ╚═══════════════════════════════════════════════════════╝
      │
70ms  │ Command Handler    │ Map user to UserDto
75ms  │ API Controller     │ Return 201 Created
80ms  │ Client             │ Receive response
```

---

## 📊 Memory State at Each Step

### After `User.Create()`

```csharp
user = {
    Id = Guid("123-456..."),
    Email = Email { Value = "john.doe@example.com" },
    FullName = "John Doe",
    IsActive = true,
    _domainEvents = [
        UserCreatedEvent {
            UserId = Guid("123-456..."),
            TenantId = Guid("tenant-1..."),
            Email = "john.doe@example.com",
            OccurredOnUtc = DateTime(2026-04-02T10:30:00Z)
        }
    ]
}
```

### After `SaveChangesAsync()` Completes

```csharp
// Database:
// Users table now has a new row

// Memory:
user._domainEvents = [] // Cleared

// Event Handlers:
// ✅ All 3 handlers have executed
// ✅ Email sent
// ✅ Audit logged
// ✅ Cache invalidated
```

---

## ❓ Common Questions

### **Q: When is the event dispatched?**
**A:** In `UnitOfWork.SaveChangesAsync()`, **AFTER** the database save succeeds.

### **Q: What if SaveChanges fails?**
**A:** Events are **NOT** dispatched. The transaction rolls back, and handlers never execute.

### **Q: Can I have multiple handlers for the same event?**
**A:** Yes! All `INotificationHandler<UserCreatedEvent>` implementations will execute.

### **Q: Do handlers run in parallel or sequential?**
**A:** **Parallel** by default. MediatR publishes to all handlers simultaneously.

### **Q: What if a handler throws an exception?**
**A:** Other handlers still execute. It's best practice to catch exceptions in handlers.

### **Q: Where are events stored before dispatching?**
**A:** In the `AggregateRoot._domainEvents` list (in-memory, not persisted).

### **Q: Why not dispatch events immediately in User.Create()?**
**A:** Because we want to dispatch **only if** the database save succeeds. This ensures consistency.

---

## 📁 File Structure Summary

```
📁 Project Structure
│
├── 🌐 WebAPI/
│   └── Program.cs                    ← STEP 9: Register MediatR
│   └── Controllers/
│       └── UsersController.cs        ← STEP 2: Receive HTTP request
│
├── 🎯 Application/
│   └── Features/Users/
│       ├── Commands/
│       │   └── CreateUserCommandHandler.cs  ← STEP 3: Orchestrate creation
│       └── EventHandlers/
│           ├── UserCreatedEventHandler.cs   ← STEP 8: Send email
│           ├── UserAuditEventHandler.cs     ← STEP 8: Log audit
│           └── UserCacheEventHandler.cs     ← STEP 8: Clear cache
│
├── 💎 Domain/
│   ├── Entities/
│   │   └── User.cs                   ← STEP 4: Raise event
│   ├── Events/
│   │   ├── IDomainEvent.cs           ← STEP 5: Marker interface
│   │   └── UserCreatedEvent.cs       ← STEP 5: Event definition
│   └── Common/
│       └── AggregateRoot.cs          ← STEP 4: Store events
│
└── 🔧 Infrastructure/
    └── Persistence/
        ├── UnitOfWork.cs              ← STEP 7: Save & dispatch
        └── Repositories/
            └── UserRepository.cs      ← STEP 6: Track entity
```

---

## 🎓 Key Takeaways

1. **IDomainEvent is a marker interface** - It identifies objects as domain events for MediatR

2. **Events are raised in the domain layer** - `User.Create()` calls `AddDomainEvent()`

3. **Events are stored in-memory** - `AggregateRoot._domainEvents` list

4. **Events are dispatched AFTER database save** - In `UnitOfWork.SaveChangesAsync()`

5. **MediatR publishes to all handlers** - `_mediator.Publish(domainEvent)`

6. **Handlers are auto-discovered** - Classes implementing `INotificationHandler<T>`

7. **Handlers execute in parallel** - All registered handlers run simultaneously

8. **This ensures consistency** - Events only dispatch if database save succeeds

---

**This is the complete, concrete flow of Domain Events in your DDD application!** 🎉
