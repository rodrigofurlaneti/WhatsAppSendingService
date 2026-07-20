namespace WhatsAppSendingService.Infrastructure.BackgroundJobs;

public sealed class OutboxDispatcherOptions
{
    public const string SectionName = "WhatsApp:OutboxDispatcher";

    public bool Enabled { get; init; } = true;
    public int PollingIntervalSeconds { get; init; } = 10;
    public int BatchSize { get; init; } = 20;
}
