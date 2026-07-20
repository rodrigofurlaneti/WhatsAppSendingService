namespace WhatsAppSendingService.Application.Abstractions.Ports;

/// <summary>Outbound port for delivering a WhatsApp message. Implemented by an
/// interchangeable adapter in Infrastructure (Cloud API, gateway, fake, ...).
/// The application core never knows which provider is behind it.</summary>
public interface IWhatsAppSender
{
    Task<WhatsAppSendResult> SendTextAsync(
        string recipientPhoneNumber,
        string body,
        CancellationToken cancellationToken = default);
}

/// <summary>Result of a delivery attempt from the sender adapter.</summary>
public sealed record WhatsAppSendResult(bool Success, string? ProviderMessageId, string? Error)
{
    public static WhatsAppSendResult Ok(string providerMessageId) => new(true, providerMessageId, null);
    public static WhatsAppSendResult Fail(string error) => new(false, null, error);
}
