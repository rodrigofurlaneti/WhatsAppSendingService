using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WhatsAppSendingService.Application;
using WhatsAppSendingService.Application.Abstractions.Ports;
using WhatsAppSendingService.Domain.Common;
using WhatsAppSendingService.Domain.Messages.Repositories;
using WhatsAppSendingService.Infrastructure.Persistence;
using WhatsAppSendingService.Infrastructure.Persistence.Repositories;
using WhatsAppSendingService.Infrastructure.Time;

namespace WhatsAppSendingService.BddTests.Support;

/// <summary>Builds a real application pipeline (MediatR + validation + handlers)
/// on top of an in-memory database and a fake provider. One instance per scenario
/// (Reqnroll context injection), guaranteeing isolation between scenarios.</summary>
public sealed class TestWorld : IDisposable
{
    private readonly ServiceProvider _provider;

    public ConfigurableFakeSender Sender { get; } = new();

    public TestWorld()
    {
        // Stable name so every scope in this scenario shares the same store.
        var dbName = $"bdd-{Guid.NewGuid()}";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();

        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));

        services.AddScoped<IWhatsAppMessageRepository, WhatsAppMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IWhatsAppSender>(Sender);

        _provider = services.BuildServiceProvider();
    }

    /// <summary>Sends a request through a fresh DI scope, mirroring how the API/host
    /// resolves a scoped pipeline per request.</summary>
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        return await mediator.Send(request);
    }

    public async Task<TEntity?> QueryAsync<TEntity>(Func<ApplicationDbContext, Task<TEntity?>> query)
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await query(db);
    }

    public void Dispose() => _provider.Dispose();
}
