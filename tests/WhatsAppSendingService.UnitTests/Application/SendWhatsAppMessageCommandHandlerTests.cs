using FluentAssertions;
using NSubstitute;
using WhatsAppSendingService.Application.Abstractions.Ports;
using WhatsAppSendingService.Application.Messages.Commands.SendMessage;
using WhatsAppSendingService.Domain.Messages;
using WhatsAppSendingService.Domain.Messages.Repositories;
using WhatsAppSendingService.UnitTests.TestDoubles;
using Xunit;

namespace WhatsAppSendingService.UnitTests.Application;

public sealed class SendWhatsAppMessageCommandHandlerTests
{
    private readonly IWhatsAppMessageRepository _repository = Substitute.For<IWhatsAppMessageRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _clock = new FixedClock(new DateTime(2026, 07, 19, 12, 0, 0, DateTimeKind.Utc));

    private SendWhatsAppMessageCommandHandler CreateSut() => new(_repository, _unitOfWork, _clock);

    [Fact]
    public async Task Handle_persists_a_pending_message_and_returns_id()
    {
        var handler = CreateSut();

        var result = await handler.Handle(
            new SendWhatsAppMessageCommand("+55 11 99999-8888", "Hello!"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.MessageId.Should().NotBeEmpty();
        result.Value.Status.Should().Be(MessageStatus.Pending.ToString());

        await _repository.Received(1).AddAsync(
            Arg.Is<WhatsAppMessage>(m => m.Content.Value == "Hello!"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_validation_failure_for_bad_phone()
    {
        var handler = CreateSut();

        var result = await handler.Handle(new SendWhatsAppMessageCommand("123", "Hi"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        await _repository.DidNotReceive().AddAsync(Arg.Any<WhatsAppMessage>(), Arg.Any<CancellationToken>());
    }
}
