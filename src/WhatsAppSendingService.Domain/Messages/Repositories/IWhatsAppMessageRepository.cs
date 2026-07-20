namespace WhatsAppSendingService.Domain.Messages.Repositories;

/// <summary>Persistence port for the WhatsAppMessage aggregate.
/// Defined in the Domain, implemented in Infrastructure (Dependency Inversion).</summary>
public interface IWhatsAppMessageRepository
{
    Task AddAsync(WhatsAppMessage message, CancellationToken cancellationToken = default);
    Task<WhatsAppMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WhatsAppMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    void Update(WhatsAppMessage message);
}
