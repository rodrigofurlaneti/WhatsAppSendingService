using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Domain.Messages.ValueObjects;

public sealed class MessageContent : ValueObject
{
    public const int MaxLength = 4096; // WhatsApp text body hard limit.

    public string Value { get; }

    private MessageContent(string value) => Value = value;

    public static Result<MessageContent> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Failure<MessageContent>(Error.Validation("Message body is required."));

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxLength)
            return Result.Failure<MessageContent>(
                Error.Validation($"Message body cannot exceed {MaxLength} characters."));

        return new MessageContent(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
