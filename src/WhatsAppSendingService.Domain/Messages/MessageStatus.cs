namespace WhatsAppSendingService.Domain.Messages;

public enum MessageStatus
{
    Pending = 0,
    Sending = 1,
    Sent = 2,
    Failed = 3
}
