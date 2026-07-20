using WhatsAppSendingService.Application.Abstractions.Messaging;
using WhatsAppSendingService.Domain.Common;
using WhatsAppSendingService.Domain.Messages.Repositories;

namespace WhatsAppSendingService.Application.Messages.Queries.GetMessageById;

internal sealed class GetMessageByIdQueryHandler(IWhatsAppMessageRepository repository)
    : IQueryHandler<GetMessageByIdQuery, MessageDto>
{
    public async Task<Result<MessageDto>> Handle(
        GetMessageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var message = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (message is null)
            return Result.Failure<MessageDto>(
                Error.NotFound($"Message '{request.Id}' was not found."));

        return Result.Success(new MessageDto(
            message.Id,
            message.Recipient.Value,
            message.Content.Value,
            message.Status.ToString(),
            message.ProviderMessageId,
            message.FailureReason,
            message.AttemptCount,
            message.CreatedOnUtc,
            message.SentOnUtc));
    }
}
