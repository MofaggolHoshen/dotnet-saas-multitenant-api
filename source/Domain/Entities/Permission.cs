using Domain.Common;

namespace Domain.Entities;

public sealed class Permission : BaseEntity
{
    private Permission(string name, string resource, string action) : base()
    {
        Name = name;
        Resource = resource;
        Action = action;
    }

    public string Name { get; private set; }
    public string Resource { get; private set; }
    public string Action { get; private set; }

    public static Permission Of(string resource, string action)
    {
        var normalizedResource = resource.Trim().ToLowerInvariant();
        var normalizedAction = action.Trim().ToLowerInvariant();
        return new Permission($"{normalizedResource}:{normalizedAction}", normalizedResource, normalizedAction);
    }

    public static Permission UsersRead() => Of("users", "read");
    public static Permission UsersWrite() => Of("users", "write");
    public static Permission TenantsManage() => Of("tenants", "manage");
}
