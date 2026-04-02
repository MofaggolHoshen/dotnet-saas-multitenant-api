namespace Domain.Exceptions;

public sealed class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(Guid tenantId)
        : base($"Tenant '{tenantId}' was not found.")
    {
        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}
