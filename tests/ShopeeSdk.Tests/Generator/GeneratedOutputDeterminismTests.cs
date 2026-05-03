using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class GeneratedOutputDeterminismTests
{
    [Fact]
    public void CoverageWriter_IsDeterministicForSchemaFixtures()
    {
        var endpoints = new SchemaReader().ReadDirectory(SchemaFixturePaths.FindSchemas());
        var writer = new EndpointCoverageWriter();

        var first = writer.WriteMarkdown(endpoints);
        var second = writer.WriteMarkdown(endpoints);

        first.Should().Be(second);
        first.Should().Contain("Total schema endpoints: 4");
    }

    [Fact]
    public void ContractsWriter_IsDeterministicForProductModuleFixture()
    {
        var endpoints = new SchemaReader()
            .ReadDirectory(SchemaFixturePaths.FindSchemas())
            .Where(endpoint => endpoint.ModuleKey == "product")
            .ToArray();

        var writer = new ContractsWriter();
        var first = writer.WriteModule("Product", "product", endpoints);
        var second = writer.WriteModule("Product", "product", endpoints);

        first.Should().Be(second);
        first.Should().Contain("public sealed record ProductGetCategoryRequest");
    }
}
