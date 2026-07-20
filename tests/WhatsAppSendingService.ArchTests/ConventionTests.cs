using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using WhatsAppSendingService.Domain.Common;
using Xunit;

namespace WhatsAppSendingService.ArchTests;

/// <summary>Guards naming/design conventions that keep the codebase consistent.</summary>
public sealed class ConventionTests
{
    [Fact]
    public void MediatR_handlers_should_be_sealed()
    {
        var result = Types.InAssembly(ArchitectureReference.Application)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Failing(result));
    }

    [Fact]
    public void Domain_events_should_be_sealed_and_named_with_suffix()
    {
        var result = Types.InAssembly(ArchitectureReference.Domain)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .And()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Failing(result));
    }

    [Fact]
    public void Value_objects_should_be_sealed()
    {
        var result = Types.InAssembly(ArchitectureReference.Domain)
            .That()
            .Inherit(typeof(ValueObject))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Failing(result));
    }

    [Fact]
    public void Repository_interfaces_should_live_in_the_domain()
    {
        var result = Types.InAssembly(ArchitectureReference.Domain)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreInterfaces()
            .Should()
            .ResideInNamespaceStartingWith(ArchitectureReference.DomainNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Failing(result));
    }

    private static string Failing(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Offending types: " + string.Join(", ", result.FailingTypeNames ?? new List<string>());
}
