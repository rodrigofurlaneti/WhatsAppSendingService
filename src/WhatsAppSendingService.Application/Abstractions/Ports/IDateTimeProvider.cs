namespace WhatsAppSendingService.Application.Abstractions.Ports;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
