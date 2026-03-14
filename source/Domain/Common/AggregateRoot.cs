using Domain.Events;

namespace Domain.Common;

public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(Guid? id = null) : base(id)
    {
    }

    protected void AddDomainEvent(IDomainEvent @event)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}