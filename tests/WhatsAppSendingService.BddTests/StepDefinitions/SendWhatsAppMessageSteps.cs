using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using WhatsAppSendingService.Application.Messages.Commands.DispatchPendingMessages;
using WhatsAppSendingService.Application.Messages.Commands.SendMessage;
using WhatsAppSendingService.BddTests.Support;
using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.BddTests.StepDefinitions;

[Binding]
public sealed class SendWhatsAppMessageSteps(TestWorld world)
{
    private Result<SendWhatsAppMessageResponse>? _sendResult;
    private Guid _messageId;

    [Given("the WhatsApp provider is available")]
    public void GivenProviderAvailable() => world.Sender.ShouldSucceed = true;

    [Given("the WhatsApp provider is failing with reason \"(.*)\"")]
    public void GivenProviderFailing(string reason)
    {
        world.Sender.ShouldSucceed = false;
        world.Sender.FailureReason = reason;
    }

    [When("I submit a message to \"(.*)\" with body \"(.*)\"")]
    public async Task WhenISubmit(string to, string body)
    {
        _sendResult = await world.SendAsync(new SendWhatsAppMessageCommand(to, body));
        if (_sendResult.IsSuccess)
            _messageId = _sendResult.Value.MessageId;
    }

    [Then("the request is accepted with status \"(.*)\"")]
    public void ThenAcceptedWith(string status)
    {
        _sendResult!.IsSuccess.Should().BeTrue();
        _sendResult.Value.Status.Should().Be(status);
    }

    [Then("the request is rejected with a validation error")]
    public void ThenRejected()
    {
        _sendResult!.IsFailure.Should().BeTrue();
        _sendResult.Error.Code.Should().Be("Validation");
    }

    [When("the outbox dispatcher runs")]
    public async Task WhenDispatcherRuns() =>
        await world.SendAsync(new DispatchPendingMessagesCommand());

    [Then("the message status becomes \"(.*)\"")]
    public async Task ThenStatusBecomes(string status)
    {
        var message = await world.QueryAsync(db =>
            db.Messages.FirstOrDefaultAsync(m => m.Id == _messageId));
        message.Should().NotBeNull();
        message!.Status.ToString().Should().Be(status);
    }

    [Then("the provider received (.*) message")]
    public void ThenProviderReceived(int count) =>
        world.Sender.Received.Should().HaveCount(count);

    [Then("the failure reason is \"(.*)\"")]
    public async Task ThenFailureReason(string reason)
    {
        var message = await world.QueryAsync(db =>
            db.Messages.FirstOrDefaultAsync(m => m.Id == _messageId));
        message!.FailureReason.Should().Be(reason);
    }
}
