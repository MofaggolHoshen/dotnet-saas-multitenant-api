using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class User : AggregateRoot
{
    private readonly HashSet<Guid> _roleIds = new();

    private User(TenantId tenantId, Email email, string passwordHash, string fullName) : base()
    {
        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        IsActive = true;
    }

    public TenantId TenantId { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<Guid> RoleIds => _roleIds;

    public static Result<User> Create(TenantId tenantId, Email email, string passwordHash, string fullName)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result<User>.Failure(Error.Validation("Password hash is required."));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result<User>.Failure(Error.Validation("Full name is required."));
        }

        var user = new User(tenantId, email, passwordHash, fullName.Trim());
        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.TenantId.Value, user.Email.Value));
        return Result<User>.Success(user);
    }

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

        if (_roleIds.Add(roleId))
        {
            MarkUpdated();
            AddDomainEvent(new RoleAssignedEvent(Id, roleId, TenantId.Value));
        }

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;
        MarkUpdated();
        AddDomainEvent(new UserDeactivatedEvent(Id, TenantId.Value));
        return Result.Success();
    }

    public Result UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            return Result.Failure(Error.Validation("New password hash is required."));
        }

        PasswordHash = newPasswordHash;
        MarkUpdated();
        AddDomainEvent(new PasswordChangedEvent(Id, TenantId.Value));
        return Result.Success();
    }
}