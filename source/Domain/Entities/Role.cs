using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Role : AggregateRoot
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    private Role(TenantId tenantId, string name, bool isSystemRole) : base()
    {
        TenantId = tenantId;
        Name = name;
        IsSystemRole = isSystemRole;
    }

    public TenantId TenantId { get; private set; }
    public string Name { get; private set; }
    public bool IsSystemRole { get; private set; }
    public IReadOnlyCollection<string> Permissions => _permissions;

    public static Result<Role> Create(TenantId tenantId, string name, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Role>.Failure(Error.Validation("Role name is required."));
        }

        return Result<Role>.Success(new Role(tenantId, name.Trim(), isSystemRole));
    }

    public Result AssignPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return Result.Failure(Error.Validation("Permission is required."));
        }

        if (_permissions.Add(permission.Trim()))
        {
            MarkUpdated();
        }

        return Result.Success();
    }

    public Result RevokePermission(string permission)
    {
        if (IsSystemRole)
        {
            return Result.Failure(Error.Conflict("Cannot revoke permissions from a system role."));
        }

        if (_permissions.Remove(permission.Trim()))
        {
            MarkUpdated();
        }

        return Result.Success();
    }
}
