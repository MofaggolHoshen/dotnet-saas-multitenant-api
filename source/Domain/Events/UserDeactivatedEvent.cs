using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events;

public sealed record UserDeactivatedEvent(Guid UserId, Guid TenantId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
