using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events;

public sealed record UserCreatedEvent(Guid UserId, Guid TenantId, string Email) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
