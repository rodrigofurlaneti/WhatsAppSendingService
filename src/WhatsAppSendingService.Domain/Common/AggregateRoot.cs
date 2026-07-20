namespace WhatsAppSendingService.Domain.Common;

/// <summary>Marks the consistency boundary of the aggregate.</summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }
}
