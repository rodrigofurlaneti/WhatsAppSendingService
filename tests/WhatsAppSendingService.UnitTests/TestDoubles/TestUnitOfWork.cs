using WhatsAppSendingService.Application.Abstractions.Ports;
using WhatsAppSendingService.Infrastructure.Persistence;

namespace WhatsAppSendingService.UnitTests.TestDoubles;

public sealed class TestUnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
