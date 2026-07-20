namespace WhatsAppSendingService.Api.Contracts;

public sealed record SendMessageResponseDto(Guid MessageId, string Status);
