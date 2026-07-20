using Microsoft.EntityFrameworkCore;
using WhatsAppSendingService.Domain.Messages;

namespace WhatsAppSendingService.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<WhatsAppMessage> Messages => Set<WhatsAppMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
