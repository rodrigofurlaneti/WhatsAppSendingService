namespace WhatsAppSendingService.Domain.Common;

/// <summary>Marker for a domain event. Kept free of any framework
/// dependency so the Domain layer references nothing external.
/// The Application layer adapts these to MediatR notifications for dispatch.</summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
