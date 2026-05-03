using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class GeneratedManagerOutputTests
{
    [Fact]
    public void GeneratedManagerInterfaces_AreDeterministicForSchemaFixtures()
    {
        var endpoints = new SchemaReader().ReadDirectory(SchemaFixturePaths.FindSchemas());
        var writer = new ManagersWriter();
        var product = endpoints.Where(endpoint => endpoint.ModuleKey == "product").ToArray();

        var first = writer.WriteInterface("Product", "product", product);
        var second = writer.WriteInterface("Product", "product", product);

        first.Should().Be(second);
        first.Should().Contain("GetCategoryAsync(long shopId, ProductGetCategoryRequest request, CancellationToken cancellationToken)");
    }

    [Fact]
    public void GeneratedManagerImplementations_AreDeterministicForSchemaFixtures()
    {
        var endpoints = new SchemaReader().ReadDirectory(SchemaFixturePaths.FindSchemas());
        var writer = new ManagersWriter();
        var media = endpoints.Where(endpoint => endpoint.ModuleKey == "media").ToArray();

        var first = writer.WriteImplementation("Media", "media", media);
        var second = writer.WriteImplementation("Media", "media", media);

        first.Should().Be(second);
        first.Should().Contain("ShopeeRequestContentKind.Multipart");
    }
}
