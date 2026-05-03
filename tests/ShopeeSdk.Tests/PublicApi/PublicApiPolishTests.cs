using System.Reflection;
using FluentAssertions;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Generated.Product;

namespace ShopeeSdk.Tests.PublicApi;

public sealed class PublicApiPolishTests
{
    private static readonly Assembly[] ShippingAssemblies =
    [
        typeof(ShopeeOptions).Assembly,
        typeof(ShopeeClient).Assembly,
        typeof(ProductGetCategoryRequest).Assembly
    ];

    [Fact]
    public void ShippingAssemblies_DoNotExposeGeneratorOrTestConcepts()
    {
        var publicTypeNames = ShippingAssemblies
            .SelectMany(assembly => assembly.ExportedTypes)
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        publicTypeNames.Should().NotContain(name => name.Contains(".Generator.", StringComparison.Ordinal));
        publicTypeNames.Should().NotContain(name => name.Contains(".Tests.", StringComparison.Ordinal));
        publicTypeNames.Should().NotContain(name => name.Contains("TestDouble", StringComparison.Ordinal));
        publicTypeNames.Should().NotContain(name => name.Contains("Fake", StringComparison.Ordinal));
        publicTypeNames.Should().NotContain(name => name.Contains("Mock", StringComparison.Ordinal));
    }

    [Fact]
    public void GeneratedShippingTypes_StayInGeneratedNamespace()
    {
        var generatedAssembly = typeof(ProductGetCategoryRequest).Assembly;

        generatedAssembly.ExportedTypes
            .Select(type => type.Namespace)
            .Distinct()
            .Should()
            .OnlyContain(ns => ns != null && ns.StartsWith("ShopeeSdk.Generated.", StringComparison.Ordinal));
    }

    [Fact]
    public void GeneratedManagerInterfaces_UseApiSuffixOnce()
    {
        var abstractionAssembly = typeof(ShopeeOptions).Assembly;
        var generatedManagerInterfaces = abstractionAssembly.ExportedTypes
            .Where(type => type.Namespace == "ShopeeSdk.Abstractions.Managers.Generated")
            .Where(type => type.IsInterface)
            .Select(type => type.Name)
            .ToArray();

        generatedManagerInterfaces.Should().OnlyContain(name => name.StartsWith("I", StringComparison.Ordinal));
        generatedManagerInterfaces.Should().OnlyContain(name => name.EndsWith("Api", StringComparison.Ordinal));
        generatedManagerInterfaces.Should().NotContain(name => name.EndsWith("ApiApi", StringComparison.Ordinal));
    }
}
