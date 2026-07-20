using WhatsAppSendingService.Application.Abstractions.Messaging;
using WhatsAppSendingService.Application.Abstractions.Ports;
using WhatsAppSendingService.Domain.Common;
using WhatsAppSendingService.Domain.Messages;
using WhatsAppSendingService.Domain.Messages.Repositories;
using WhatsAppSendingService.Domain.Messages.ValueObjects;

namespace WhatsAppSendingService.Application.Messages.Commands.SendMessage;

/// <summary>Use case: accept a request and persist it as a queued message
/// (transactional outbox). Actual delivery is performed asynchronously by the
/// dispatcher, so the API responds fast and delivery is retriable/decoupled.</summary>
internal sealed class SendWhatsAppMessageCommandHandler(
    IWhatsAppMessageRepository repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : ICommandHandler<SendWhatsAppMessageCommand, SendWhatsAppMessageResponse>
{
    public async Task<Result<SendWhatsAppMessageResponse>> Handle(
        SendWhatsAppMessageCommand request,
        CancellationToken cancellationToken)
    {
        var phoneResult = PhoneNumber.Create(request.To);
        if (phoneResult.IsFailure)
            return Result.Failure<SendWhatsAppMessageResponse>(phoneResult.Error);

        var contentResult = MessageContent.Create(request.Body);
        if (contentResult.IsFailure)
            return Result.Failure<SendWhatsAppMessageResponse>(contentResult.Error);

        var message = WhatsAppMessage.Queue(phoneResult.Value, contentResult.Value, clock.UtcNow);

        await repository.AddAsync(message, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            SendWhatsAppMessageResponse.From(message.Id, message.Status));
    }
}
