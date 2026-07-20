using WhatsAppSendingService.Application.Abstractions.Messaging;

namespace WhatsAppSendingService.Application.Messages.Queries.GetMessageById;

public sealed record GetMessageByIdQuery(Guid Id) : IQuery<MessageDto>;

public sealed record MessageDto(
    Guid Id,
    string To,
    string Body,
    string Status,
    string? ProviderMessageId,
    string? FailureReason,
    int AttemptCount,
    DateTime CreatedOnUtc,
    DateTime? SentOnUtc);
