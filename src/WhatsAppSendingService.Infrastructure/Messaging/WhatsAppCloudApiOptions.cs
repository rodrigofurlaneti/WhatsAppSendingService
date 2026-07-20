using System.ComponentModel.DataAnnotations;

namespace WhatsAppSendingService.Infrastructure.Messaging;

public sealed class WhatsAppCloudApiOptions
{
    public const string SectionName = "WhatsApp:CloudApi";

    /// <summary>Graph API base, e.g. https://graph.facebook.com</summary>
    [Required]
    public string BaseUrl { get; init; } = "https://graph.facebook.com";

    [Required]
    public string ApiVersion { get; init; } = "v21.0";

    /// <summary>The business phone number id registered in Meta.</summary>
    [Required]
    public string PhoneNumberId { get; init; } = string.Empty;

    /// <summary>Permanent/system-user access token.</summary>
    [Required]
    public string AccessToken { get; init; } = string.Empty;
}
