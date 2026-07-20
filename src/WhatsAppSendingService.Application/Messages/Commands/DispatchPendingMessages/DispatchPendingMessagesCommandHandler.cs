using Microsoft.Extensions.Logging;
using WhatsAppSendingService.Application.Abstractions.Messaging;
using WhatsAppSendingService.Application.Abstractions.Ports;
using WhatsAppSendingService.Domain.Common;
using WhatsAppSendingService.Domain.Messages.Repositories;

namespace WhatsAppSendingService.Application.Messages.Commands.DispatchPendingMessages;

internal sealed class DispatchPendingMessagesCommandHandler(
    IWhatsAppMessageRepository repository,
    IWhatsAppSender sender,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    ILogger<DispatchPendingMessagesCommandHandler> logger)
    : ICommandHandler<DispatchPendingMessagesCommand, DispatchPendingMessagesResult>
{
    public async Task<Result<DispatchPendingMessagesResult>> Handle(
        DispatchPendingMessagesCommand request,
        CancellationToken cancellationToken)
    {
        var pending = await repository.GetPendingAsync(request.BatchSize, cancellationToken);
        int sent = 0, failed = 0;

        foreach (var message in pending)
        {
            message.MarkAsSending();

            WhatsAppSendResult result;
            try
            {
                result = await sender.SendTextAsync(
                    message.Recipient.Value, message.Content.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error sending message {MessageId}", message.Id);
                result = WhatsAppSendResult.Fail(ex.Message);
            }

            if (result.Success)
            {
                message.MarkAsSent(result.ProviderMessageId!, clock.UtcNow);
                sent++;
            }
            else
            {
                message.MarkAsFailed(result.Error ?? "Unknown delivery error.");
                failed++;
            }

            repository.Update(message);
        }

        if (pending.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DispatchPendingMessagesResult(pending.Count, sent, failed));
    }
}
