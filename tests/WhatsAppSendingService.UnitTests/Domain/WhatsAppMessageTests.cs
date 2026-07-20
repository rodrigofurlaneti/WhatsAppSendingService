using FluentAssertions;
using WhatsAppSendingService.Domain.Exceptions;
using WhatsAppSendingService.Domain.Messages;
using WhatsAppSendingService.Domain.Messages.Events;
using WhatsAppSendingService.Domain.Messages.ValueObjects;
using Xunit;

namespace WhatsAppSendingService.UnitTests.Domain;

public sealed class WhatsAppMessageTests
{
    private static WhatsAppMessage NewMessage()
    {
        var phone = PhoneNumber.Create("5511999998888").Value;
        var content = MessageContent.Create("hi").Value;
        return WhatsAppMessage.Queue(phone, content, DateTime.UtcNow);
    }

    [Fact]
    public void Queue_starts_pending_and_raises_queued_event()
    {
        var message = NewMessage();

        message.Status.Should().Be(MessageStatus.Pending);
        message.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<WhatsAppMessageQueuedDomainEvent>();
    }

    [Fact]
    public void Full_success_lifecycle_transitions_to_sent()
    {
        var message = NewMessage();

        message.MarkAsSending();
        message.Status.Should().Be(MessageStatus.Sending);
        message.AttemptCount.Should().Be(1);

        message.MarkAsSent("wamid.123", DateTime.UtcNow);

        message.Status.Should().Be(MessageStatus.Sent);
        message.ProviderMessageId.Should().Be("wamid.123");
        message.SentOnUtc.Should().NotBeNull();
        message.DomainEvents.Should().Contain(e => e is WhatsAppMessageSentDomainEvent);
    }

    [Fact]
    public void MarkAsSent_without_sending_state_throws()
    {
        var message = NewMessage();

        var act = () => message.MarkAsSent("wamid.1", DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsFailed_then_requeue_returns_to_pending()
    {
        var message = NewMessage();
        message.MarkAsSending();
        message.MarkAsFailed("network error");

        message.Status.Should().Be(MessageStatus.Failed);
        message.FailureReason.Should().Be("network error");

        message.Requeue();
        message.Status.Should().Be(MessageStatus.Pending);
        message.FailureReason.Should().BeNull();
    }

    [Fact]
    public void Sent_message_cannot_be_resent()
    {
        var message = NewMessage();
        message.MarkAsSending();
        message.MarkAsSent("wamid.1", DateTime.UtcNow);

        var act = () => message.MarkAsSending();

        act.Should().Throw<DomainException>();
    }
}
