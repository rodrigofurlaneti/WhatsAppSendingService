using WhatsAppSendingService.Application.Abstractions.Ports;

namespace WhatsAppSendingService.Infrastructure.Time;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
