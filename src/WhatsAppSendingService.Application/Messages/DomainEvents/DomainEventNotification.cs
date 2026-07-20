using MediatR;
using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Application.Messages.DomainEvents;

/// <summary>Wraps a pure Domain event as a MediatR notification so it can be
/// published without the Domain layer referencing MediatR.</summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
