using Application.Common.Interfaces;

namespace Infrastructure.Multitenancy;

public sealed class UnresolvedTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;
    public string? TenantName => null;
    public bool IsResolved => false;
}
