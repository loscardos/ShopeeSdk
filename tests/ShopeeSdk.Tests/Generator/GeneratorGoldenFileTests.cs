using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class GeneratorGoldenFileTests
{
    [Fact]
    public void ContractsWriter_GeneratesProductRequestAndResponseRecords()
    {
        var endpoint = new SchemaEndpoint(
            "v2.product.get_category",
            "Product",
            "product",
            "/product/get_category",
            ShopeeSchemaHttpMethod.Get,
            ShopeeSchemaAuthScope.Shop,
            ShopeeSchemaRequestContentKind.Query,
            [new SchemaParameter("language", "string", false, [])],
            [new SchemaParameter("response", "object", false, [
                new SchemaParameter("category_list", "object[]", false, [
                    new SchemaParameter("category_id", "int64", false, []),
                    new SchemaParameter("display_category_name", "string", false, [])
                ])
            ])]);

        var output = new ContractsWriter().WriteModule("Product", "product", [endpoint]);

        output.Should().Contain("namespace ShopeeSdk.Generated.Product;");
        output.Should().Contain("public sealed record ProductGetCategoryRequest(");
        output.Should().Contain("[property: JsonPropertyName(\"language\")] string? Language = null");
        output.Should().Contain("public sealed record ProductGetCategoryResponse(");
        output.Should().Contain("IReadOnlyList<ProductGetCategoryResponseCategoryListItem>? CategoryList = null");
    }
}
