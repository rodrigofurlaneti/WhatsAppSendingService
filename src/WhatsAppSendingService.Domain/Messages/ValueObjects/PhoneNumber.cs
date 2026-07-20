using System.Text.RegularExpressions;
using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Domain.Messages.ValueObjects;

/// <summary>E.164 phone number (digits only, with country code, no '+').
/// WhatsApp Cloud API expects the recipient in this normalized form.</summary>
public sealed partial class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static Result<PhoneNumber> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<PhoneNumber>(Error.Validation("Phone number is required."));

        // Keep only digits (strip +, spaces, dashes, parentheses).
        var digits = NonDigits().Replace(raw, string.Empty);

        if (digits.Length is < 8 or > 15)
            return Result.Failure<PhoneNumber>(
                Error.Validation("Phone number must contain between 8 and 15 digits (E.164)."));

        return new PhoneNumber(digits);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex NonDigits();
}
