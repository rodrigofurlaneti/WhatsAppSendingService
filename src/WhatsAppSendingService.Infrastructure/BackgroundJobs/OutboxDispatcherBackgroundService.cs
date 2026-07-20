using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WhatsAppSendingService.Application.Messages.Commands.DispatchPendingMessages;

namespace WhatsAppSendingService.Infrastructure.BackgroundJobs;

/// <summary>Periodically triggers the DispatchPendingMessages use case so queued
/// messages are delivered autonomously, decoupled from the HTTP request.</summary>
internal sealed class OutboxDispatcherBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxDispatcherOptions> options,
    ILogger<OutboxDispatcherBackgroundService> logger)
    : BackgroundService
{
    private readonly OutboxDispatcherOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Outbox dispatcher is disabled by configuration.");
            return;
        }

        var delay = TimeSpan.FromSeconds(Math.Max(1, _options.PollingIntervalSeconds));
        logger.LogInformation("Outbox dispatcher started (interval: {Delay}).", delay);

        using var timer = new PeriodicTimer(delay);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await sender.Send(
                    new DispatchPendingMessagesCommand(_options.BatchSize), stoppingToken);

                if (result.IsSuccess && result.Value.Processed > 0)
                    logger.LogInformation(
                        "Dispatched {Processed} message(s): {Sent} sent, {Failed} failed.",
                        result.Value.Processed, result.Value.Sent, result.Value.Failed);
            }
            catch (OperationCanceledException) { /* shutting down */ }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatch cycle failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
