using System.Reflection;

namespace WhatsAppSendingService.ArchTests;

/// <summary>Central handle to each layer's assembly + namespace root,
/// so architecture rules stay readable.</summary>
public static class ArchitectureReference
{
    public const string DomainNamespace = "WhatsAppSendingService.Domain";
    public const string ApplicationNamespace = "WhatsAppSendingService.Application";
    public const string InfrastructureNamespace = "WhatsAppSendingService.Infrastructure";
    public const string ApiNamespace = "WhatsAppSendingService.Api";

    public static readonly Assembly Domain =
        typeof(Domain.Messages.WhatsAppMessage).Assembly;
    public static readonly Assembly Application =
        typeof(Application.DependencyInjection).Assembly;
    public static readonly Assembly Infrastructure =
        typeof(Infrastructure.DependencyInjection).Assembly;
    public static readonly Assembly Api =
        typeof(Program).Assembly;
}
