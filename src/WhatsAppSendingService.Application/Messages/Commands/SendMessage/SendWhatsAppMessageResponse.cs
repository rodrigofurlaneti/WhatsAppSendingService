using WhatsAppSendingService.Domain.Messages;

namespace WhatsAppSendingService.Application.Messages.Commands.SendMessage;

public sealed record SendWhatsAppMessageResponse(Guid MessageId, string Status)
{
    public static SendWhatsAppMessageResponse From(Guid id, MessageStatus status) =>
        new(id, status.ToString());
}
