using MediatR;

namespace Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Extends INotification to enable MediatR event publishing without coupling domain to full MediatR.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
