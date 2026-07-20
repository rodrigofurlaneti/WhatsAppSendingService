using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Domain.Messages.Events;

public sealed record WhatsAppMessageSentDomainEvent(Guid MessageId, string ProviderMessageId) : DomainEvent;
