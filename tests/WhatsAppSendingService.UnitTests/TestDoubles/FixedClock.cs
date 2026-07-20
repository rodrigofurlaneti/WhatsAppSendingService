using WhatsAppSendingService.Application.Abstractions.Ports;

namespace WhatsAppSendingService.UnitTests.TestDoubles;

public sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}
