using Domain.Abstractions;

namespace Domain.Common.DomainEvents;

public class LookupUpdatedEvent(Lookup lookup) : BaseEvent
{
    public Lookup Lookup { get; } = lookup;
}
