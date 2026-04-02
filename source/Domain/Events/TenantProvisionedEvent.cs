namespace Domain.Events;

public sealed record TenantProvisionedEvent(Guid TenantId, string Name, string Subdomain) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
