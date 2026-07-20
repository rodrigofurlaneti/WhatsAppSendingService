using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WhatsAppSendingService.Application.Messages.Commands.DispatchPendingMessages;
using WhatsAppSendingService.Domain.Messages;
using WhatsAppSendingService.Domain.Messages.ValueObjects;
using WhatsAppSendingService.Infrastructure.Persistence;
using WhatsAppSendingService.Infrastructure.Persistence.Repositories;
using WhatsAppSendingService.UnitTests.TestDoubles;
using Xunit;

namespace WhatsAppSendingService.UnitTests.Application;

public sealed class DispatchPendingMessagesCommandHandlerTests
{
    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"dispatch-{Guid.NewGuid()}")
            .Options);

    private static WhatsAppMessage Queue(string to = "5511999998888") =>
        WhatsAppMessage.Queue(
            PhoneNumber.Create(to).Value,
            MessageContent.Create("hi").Value,
            DateTime.UtcNow);

    [Fact]
    public async Task Dispatch_marks_messages_sent_when_sender_succeeds()
    {
        await using var ctx = NewContext();
        ctx.Messages.Add(Queue());
        ctx.Messages.Add(Queue("5511888887777"));
        await ctx.SaveChangesAsync();

        var sender = new FakeWhatsAppSender(succeed: true);
        var handler = new DispatchPendingMessagesCommandHandler(
            new WhatsAppMessageRepository(ctx), sender,
            new TestUnitOfWork(ctx), new FixedClock(DateTime.UtcNow),
            NullLogger<DispatchPendingMessagesCommandHandler>.Instance);

        var result = await handler.Handle(new DispatchPendingMessagesCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Processed.Should().Be(2);
        result.Value.Sent.Should().Be(2);
        sender.Sent.Should().HaveCount(2);
        (await ctx.Messages.CountAsync(m => m.Status == MessageStatus.Sent)).Should().Be(2);
    }

    [Fact]
    public async Task Dispatch_marks_message_failed_when_sender_fails()
    {
        await using var ctx = NewContext();
        ctx.Messages.Add(Queue());
        await ctx.SaveChangesAsync();

        var handler = new DispatchPendingMessagesCommandHandler(
            new WhatsAppMessageRepository(ctx),
            new FakeWhatsAppSender(succeed: false, error: "boom"),
            new TestUnitOfWork(ctx), new FixedClock(DateTime.UtcNow),
            NullLogger<DispatchPendingMessagesCommandHandler>.Instance);

        var result = await handler.Handle(new DispatchPendingMessagesCommand(), default);

        result.Value.Failed.Should().Be(1);
        var msg = await ctx.Messages.SingleAsync();
        msg.Status.Should().Be(MessageStatus.Failed);
        msg.FailureReason.Should().Be("boom");
    }
}
