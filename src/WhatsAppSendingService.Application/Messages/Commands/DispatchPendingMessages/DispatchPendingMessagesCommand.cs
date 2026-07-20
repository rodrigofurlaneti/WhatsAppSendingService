using WhatsAppSendingService.Application.Abstractions.Messaging;

namespace WhatsAppSendingService.Application.Messages.Commands.DispatchPendingMessages;

/// <summary>Picks up a batch of pending messages and attempts delivery via the
/// configured IWhatsAppSender adapter. Invoked by the background dispatcher.</summary>
public sealed record DispatchPendingMessagesCommand(int BatchSize = 20)
    : ICommand<DispatchPendingMessagesResult>;

public sealed record DispatchPendingMessagesResult(int Processed, int Sent, int Failed);
