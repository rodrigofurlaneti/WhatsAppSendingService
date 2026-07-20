using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WhatsAppSendingService.Application.Abstractions.Ports;

namespace WhatsAppSendingService.Infrastructure.Messaging;

/// <summary>Adapter that delivers messages through the official WhatsApp Cloud
/// API (Meta Graph API). Swap this class for another IWhatsAppSender to change
/// provider without touching the application core.</summary>
internal sealed class WhatsAppCloudApiSender(
    HttpClient httpClient,
    IOptions<WhatsAppCloudApiOptions> options,
    ILogger<WhatsAppCloudApiSender> logger)
    : IWhatsAppSender
{
    private readonly WhatsAppCloudApiOptions _options = options.Value;

    public async Task<WhatsAppSendResult> SendTextAsync(
        string recipientPhoneNumber,
        string body,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"{_options.ApiVersion}/{_options.PhoneNumberId}/messages";

        var payload = new CloudApiMessageRequest
        {
            To = recipientPhoneNumber,
            Text = new CloudApiTextBody { Body = body }
        };

        try
        {
            using var response = await httpClient.PostAsJsonAsync(requestUri, payload, cancellationToken);
            var content = await response.Content.ReadFromJsonAsync<CloudApiMessageResponse>(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var providerId = content?.Messages?.FirstOrDefault()?.Id;
                if (string.IsNullOrWhiteSpace(providerId))
                    return WhatsAppSendResult.Fail("Cloud API returned success without a message id.");

                return WhatsAppSendResult.Ok(providerId);
            }

            var error = content?.Error?.Message ?? $"HTTP {(int)response.StatusCode}";
            logger.LogWarning("Cloud API rejected message to {To}: {Error}", recipientPhoneNumber, error);
            return WhatsAppSendResult.Fail(error);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Transport error calling WhatsApp Cloud API.");
            return WhatsAppSendResult.Fail(ex.Message);
        }
    }

    // --- Graph API contracts ---
    private sealed class CloudApiMessageRequest
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct => "whatsapp";

        [JsonPropertyName("recipient_type")]
        public string RecipientType => "individual";

        [JsonPropertyName("to")]
        public string To { get; init; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type => "text";

        [JsonPropertyName("text")]
        public CloudApiTextBody Text { get; init; } = new();
    }

    private sealed class CloudApiTextBody
    {
        [JsonPropertyName("preview_url")]
        public bool PreviewUrl => false;

        [JsonPropertyName("body")]
        public string Body { get; init; } = string.Empty;
    }

    private sealed class CloudApiMessageResponse
    {
        [JsonPropertyName("messages")]
        public List<CloudApiMessageId>? Messages { get; init; }

        [JsonPropertyName("error")]
        public CloudApiError? Error { get; init; }
    }

    private sealed class CloudApiMessageId
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }

    private sealed class CloudApiError
    {
        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("code")]
        public int Code { get; init; }
    }
}
