using WhatsAppSendingService.Domain.Common;
using WhatsAppSendingService.Domain.Exceptions;
using WhatsAppSendingService.Domain.Messages.Events;
using WhatsAppSendingService.Domain.Messages.ValueObjects;

namespace WhatsAppSendingService.Domain.Messages;

/// <summary>Aggregate root representing a single outbound WhatsApp message
/// and its delivery lifecycle. All state transitions are guarded here so the
/// aggregate can never enter an inconsistent state (invariants live in the domain).</summary>
public sealed class WhatsAppMessage : AggregateRoot
{
    // EF Core materialization ctor.
    private WhatsAppMessage(Guid id) : base(id) { }

    private WhatsAppMessage(
        Guid id,
        PhoneNumber recipient,
        MessageContent content,
        DateTime createdOnUtc) : base(id)
    {
        Recipient = recipient;
        Content = content;
        Type = MessageType.Text;
        Status = MessageStatus.Pending;
        CreatedOnUtc = createdOnUtc;
        AttemptCount = 0;
    }

    public PhoneNumber Recipient { get; private set; } = null!;
    public MessageContent Content { get; private set; } = null!;
    public MessageType Type { get; private set; }
    public MessageStatus Status { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? FailureReason { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? SentOnUtc { get; private set; }

    /// <summary>Factory: creates a message already queued for delivery.</summary>
    public static WhatsAppMessage Queue(PhoneNumber recipient, MessageContent content, DateTime nowUtc)
    {
        var message = new WhatsAppMessage(Guid.NewGuid(), recipient, content, nowUtc);
        message.Raise(new WhatsAppMessageQueuedDomainEvent(message.Id));
        return message;
    }

    /// <summary>Marks the message as being dispatched (locks it for the current attempt).</summary>
    public void MarkAsSending()
    {
        if (Status is MessageStatus.Sent)
            throw new DomainException("A message that was already sent cannot be re-sent.");

        Status = MessageStatus.Sending;
        AttemptCount++;
    }

    public void MarkAsSent(string providerMessageId, DateTime nowUtc)
    {
        if (Status is not MessageStatus.Sending)
            throw new DomainException("Only a message in the 'Sending' state can be marked as sent.");
        if (string.IsNullOrWhiteSpace(providerMessageId))
            throw new DomainException("A provider message id is required to confirm delivery.");

        Status = MessageStatus.Sent;
        ProviderMessageId = providerMessageId;
        FailureReason = null;
        SentOnUtc = nowUtc;
        Raise(new WhatsAppMessageSentDomainEvent(Id, providerMessageId));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status is MessageStatus.Sent)
            throw new DomainException("A message that was already sent cannot be marked as failed.");

        Status = MessageStatus.Failed;
        FailureReason = reason;
        Raise(new WhatsAppMessageFailedDomainEvent(Id, reason));
    }

    /// <summary>Returns a failed message to the queue for another attempt.</summary>
    public void Requeue()
    {
        if (Status is not MessageStatus.Failed)
            throw new DomainException("Only a failed message can be requeued.");

        Status = MessageStatus.Pending;
        FailureReason = null;
    }
}
