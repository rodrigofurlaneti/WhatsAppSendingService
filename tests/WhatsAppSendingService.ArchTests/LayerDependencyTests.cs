using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace WhatsAppSendingService.ArchTests;

/// <summary>Enforces the Clean Architecture dependency rule: dependencies point
/// inward only (Api -> Infrastructure -> Application -> Domain).</summary>
public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_should_not_depend_on_any_other_layer()
    {
        var result = Types.InAssembly(ArchitectureReference.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                ArchitectureReference.ApplicationNamespace,
                ArchitectureReference.InfrastructureNamespace,
                ArchitectureReference.ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Domain_should_not_depend_on_infrastructure_frameworks()
    {
        // The Domain must stay free of EF Core, MediatR, ASP.NET, etc.
        var result = Types.InAssembly(ArchitectureReference.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "MediatR",
                "Microsoft.AspNetCore",
                "FluentValidation")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        var result = Types.InAssembly(ArchitectureReference.Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                ArchitectureReference.InfrastructureNamespace,
                ArchitectureReference.ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Application_should_not_depend_on_ef_core()
    {
        // Persistence concerns belong to Infrastructure, not the use cases.
        var result = Types.InAssembly(ArchitectureReference.Application)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(ArchitectureReference.Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureReference.ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    private static string BuildMessage(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Offending types: " + string.Join(", ", result.FailingTypeNames ?? new List<string>());
}
