using FluentAssertions;
using WhatsAppSendingService.Domain.Messages.ValueObjects;
using Xunit;

namespace WhatsAppSendingService.UnitTests.Domain;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+55 11 99999-8888", "5511999998888")]
    [InlineData("5511999998888", "5511999998888")]
    [InlineData("(55) 11 99999 8888", "5511999998888")]
    public void Create_normalizes_valid_numbers(string input, string expected)
    {
        var result = PhoneNumber.Create(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]                 // too short
    [InlineData("1234567890123456")]    // too long
    [InlineData(null)]
    public void Create_rejects_invalid_numbers(string? input)
    {
        var result = PhoneNumber.Create(input);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
