using WhatsAppSendingService.Application.Abstractions.Ports;

namespace WhatsAppSendingService.UnitTests.TestDoubles;

/// <summary>In-memory sender used across tests. Records every send and can be
/// configured to succeed or fail, standing in for a real provider adapter.</summary>
public sealed class FakeWhatsAppSender : IWhatsAppSender
{
    private readonly bool _succeed;
    private readonly string? _error;

    public FakeWhatsAppSender(bool succeed = true, string? error = null)
    {
        _succeed = succeed;
        _error = error;
    }

    public List<(string To, string Body)> Sent { get; } = new();

    public Task<WhatsAppSendResult> SendTextAsync(
        string recipientPhoneNumber, string body, CancellationToken cancellationToken = default)
    {
        Sent.Add((recipientPhoneNumber, body));
        return Task.FromResult(_succeed
            ? WhatsAppSendResult.Ok($"wamid.TEST.{Guid.NewGuid():N}")
            : WhatsAppSendResult.Fail(_error ?? "forced failure"));
    }
}
