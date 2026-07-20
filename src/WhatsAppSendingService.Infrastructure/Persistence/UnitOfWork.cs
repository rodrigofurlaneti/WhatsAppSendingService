using MediatR;
using WhatsAppSendingService.Application.Abstractions.Ports;
using WhatsAppSendingService.Application.Messages.DomainEvents;
using WhatsAppSendingService.Domain.Common;

namespace WhatsAppSendingService.Infrastructure.Persistence;

internal sealed class UnitOfWork(ApplicationDbContext context, IPublisher publisher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = context.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await context.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
            await PublishAsync(domainEvent, cancellationToken);

        return result;
    }

    private Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var notification = Activator.CreateInstance(notificationType, domainEvent)!;
        return publisher.Publish(notification, cancellationToken);
    }
}
