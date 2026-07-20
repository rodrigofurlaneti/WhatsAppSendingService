using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WhatsAppSendingService.Application.Abstractions.Ports;
using WhatsAppSendingService.Domain.Messages.Repositories;
using WhatsAppSendingService.Infrastructure.BackgroundJobs;
using WhatsAppSendingService.Infrastructure.Messaging;
using WhatsAppSendingService.Infrastructure.Persistence;
using WhatsAppSendingService.Infrastructure.Persistence.Repositories;
using WhatsAppSendingService.Infrastructure.Time;

namespace WhatsAppSendingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddWhatsAppCloudApi(configuration);
        services.AddOutboxDispatcher(configuration);

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Persistence:Provider"] ?? "SqlServer";

        if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("WhatsAppSendingService"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("Database")
                ?? "Server=localhost;Database=WhatsAppSendingService;Trusted_Connection=True;TrustServerCertificate=True;";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        }

        services.AddScoped<IWhatsAppMessageRepository, WhatsAppMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddWhatsAppCloudApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WhatsAppCloudApiOptions>()
            .Bind(configuration.GetSection(WhatsAppCloudApiOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddHttpClient<IWhatsAppSender, WhatsAppCloudApiSender>((sp, client) =>
        {
            var opts = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<WhatsAppCloudApiOptions>>().Value;
            client.BaseAddress = new Uri($"{opts.BaseUrl.TrimEnd('/')}/");
            if (!string.IsNullOrWhiteSpace(opts.AccessToken))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.AccessToken);
        });

        return services;
    }

    private static IServiceCollection AddOutboxDispatcher(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OutboxDispatcherOptions>()
            .Bind(configuration.GetSection(OutboxDispatcherOptions.SectionName));

        services.AddHostedService<OutboxDispatcherBackgroundService>();
        return services;
    }
}
