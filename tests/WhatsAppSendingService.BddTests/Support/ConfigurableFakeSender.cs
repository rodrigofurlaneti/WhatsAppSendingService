using WhatsAppSendingService.Application.Abstractions.Ports;

namespace WhatsAppSendingService.BddTests.Support;

public sealed class ConfigurableFakeSender : IWhatsAppSender
{
    public bool ShouldSucceed { get; set; } = true;
    public string FailureReason { get; set; } = "provider rejected the message";
    public List<(string To, string Body)> Received { get; } = new();

    public Task<WhatsAppSendResult> SendTextAsync(
        string recipientPhoneNumber, string body, CancellationToken cancellationToken = default)
    {
        Received.Add((recipientPhoneNumber, body));
        return Task.FromResult(ShouldSucceed
            ? WhatsAppSendResult.Ok($"wamid.BDD.{Guid.NewGuid():N}")
            : WhatsAppSendResult.Fail(FailureReason));
    }
}
