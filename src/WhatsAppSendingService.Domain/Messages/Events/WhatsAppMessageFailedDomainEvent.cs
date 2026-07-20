using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Domain.Messages.Events;

public sealed record WhatsAppMessageFailedDomainEvent(Guid MessageId, string Reason) : DomainEvent;
