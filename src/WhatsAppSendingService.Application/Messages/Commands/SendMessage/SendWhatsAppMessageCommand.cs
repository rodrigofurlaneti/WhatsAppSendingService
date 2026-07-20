using WhatsAppSendingService.Application.Abstractions.Messaging;

namespace WhatsAppSendingService.Application.Messages.Commands.SendMessage;

public sealed record SendWhatsAppMessageCommand(string To, string Body)
    : ICommand<SendWhatsAppMessageResponse>;
