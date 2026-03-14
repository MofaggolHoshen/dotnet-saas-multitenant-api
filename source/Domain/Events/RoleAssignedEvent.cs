using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events;

public sealed record RoleAssignedEvent(Guid UserId, Guid RoleId, Guid TenantId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
