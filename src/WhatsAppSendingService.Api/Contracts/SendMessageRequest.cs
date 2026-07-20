using System.ComponentModel.DataAnnotations;

namespace WhatsAppSendingService.Api.Contracts;

/// <summary>Inbound DTO. Received by the API and mapped to an application command.</summary>
public sealed class SendMessageRequest
{
    /// <summary>Recipient phone in E.164 (e.g. 5511999998888). '+' and spaces are tolerated.</summary>
    [Required]
    public string To { get; init; } = string.Empty;

    /// <summary>Text body of the WhatsApp message.</summary>
    [Required]
    public string Body { get; init; } = string.Empty;
}
