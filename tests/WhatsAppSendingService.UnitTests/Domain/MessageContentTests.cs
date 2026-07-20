using FluentAssertions;
using WhatsAppSendingService.Domain.Messages.ValueObjects;
using Xunit;

namespace WhatsAppSendingService.UnitTests.Domain;

public sealed class MessageContentTests
{
    [Fact]
    public void Create_trims_and_accepts_valid_body()
    {
        var result = MessageContent.Create("  hello  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("hello");
    }

    [Fact]
    public void Create_rejects_empty_body()
    {
        MessageContent.Create("   ").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_rejects_body_over_max_length()
    {
        var body = new string('x', MessageContent.MaxLength + 1);

        MessageContent.Create(body).IsFailure.Should().BeTrue();
    }
}
