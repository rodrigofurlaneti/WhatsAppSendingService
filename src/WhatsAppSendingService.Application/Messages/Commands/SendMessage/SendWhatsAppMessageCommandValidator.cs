using FluentValidation;
using WhatsAppSendingService.Domain.Messages.ValueObjects;

namespace WhatsAppSendingService.Application.Messages.Commands.SendMessage;

public sealed class SendWhatsAppMessageCommandValidator : AbstractValidator<SendWhatsAppMessageCommand>
{
    public SendWhatsAppMessageCommandValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Recipient phone number is required.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required.")
            .MaximumLength(MessageContent.MaxLength)
            .WithMessage($"Message body cannot exceed {MessageContent.MaxLength} characters.");
    }
}
