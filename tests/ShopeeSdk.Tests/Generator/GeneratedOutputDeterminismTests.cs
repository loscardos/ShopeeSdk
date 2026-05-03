using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class GeneratedOutputDeterminismTests
{
    [Fact]
    public void CoverageWriter_IsDeterministicForReferenceSchemas()
    {
        var endpoints = new SchemaReader().ReadDirectory(FindReferenceSchemas());
        var writer = new EndpointCoverageWriter();

        var first = writer.WriteMarkdown(endpoints);
        var second = writer.WriteMarkdown(endpoints);

        first.Should().Be(second);
        first.Should().Contain("Total schema endpoints: 425");
    }

    [Fact]
    public void ContractsWriter_IsDeterministicForProductModule()
    {
        var endpoints = new SchemaReader()
            .ReadDirectory(FindReferenceSchemas())
            .Where(endpoint => endpoint.ModuleKey == "product")
            .ToArray();

        var writer = new ContractsWriter();
        var first = writer.WriteModule("Product", "product", endpoints);
        var second = writer.WriteModule("Product", "product", endpoints);

        first.Should().Be(second);
        first.Should().Contain("public sealed record ProductGetCategoryRequest");
    }

    private static string FindReferenceSchemas()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "reference", "schemas");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find reference/schemas from test output directory.");
    }
}
