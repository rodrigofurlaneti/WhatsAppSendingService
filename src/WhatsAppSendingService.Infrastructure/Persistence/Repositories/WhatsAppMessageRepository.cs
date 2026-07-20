using Microsoft.EntityFrameworkCore;
using WhatsAppSendingService.Domain.Messages;
using WhatsAppSendingService.Domain.Messages.Repositories;

namespace WhatsAppSendingService.Infrastructure.Persistence.Repositories;

internal sealed class WhatsAppMessageRepository(ApplicationDbContext context)
    : IWhatsAppMessageRepository
{
    public async Task AddAsync(WhatsAppMessage message, CancellationToken cancellationToken = default) =>
        await context.Messages.AddAsync(message, cancellationToken);

    public async Task<WhatsAppMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Messages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WhatsAppMessage>> GetPendingAsync(
        int batchSize, CancellationToken cancellationToken = default) =>
        await context.Messages
            .Where(m => m.Status == MessageStatus.Pending)
            .OrderBy(m => m.CreatedOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public void Update(WhatsAppMessage message) => context.Messages.Update(message);
}
