using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Domain.Messages.Events;

public sealed record WhatsAppMessageQueuedDomainEvent(Guid MessageId) : DomainEvent;
