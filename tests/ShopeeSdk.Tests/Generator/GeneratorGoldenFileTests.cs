using FluentAssertions;
using Loscardos.ShopeeSdk.Generator;

namespace Loscardos.ShopeeSdk.Tests.Generator;

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

        output.Should().Contain("namespace Loscardos.ShopeeSdk.Generated.Product;");
        output.Should().Contain("public sealed record ProductGetCategoryRequest(");
        output.Should().Contain("[property: JsonPropertyName(\"language\")] string? Language = null");
        output.Should().Contain("public sealed record ProductGetCategoryResponse(");
        output.Should().Contain("IReadOnlyList<ProductGetCategoryResponseCategoryListItem>? CategoryList = null");
    }

    [Fact]
    public void ManagersWriter_GeneratesShopAuthenticatedManagerMethod()
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
            []);

        var output = new ManagersWriter().WriteInterface("Product", "product", [endpoint]);

        output.Should().Contain("public interface IProductApi");
        output.Should().Contain("Task<ProductGetCategoryResponse> GetCategoryAsync(long shopId, ProductGetCategoryRequest request, CancellationToken cancellationToken);");
    }

    [Fact]
    public void ContractsWriter_GeneratesRequiredRequestParametersWithoutNullDefaults()
    {
        var endpoint = new SchemaEndpoint(
            "v2.payment.get_escrow_detail",
            "Payment",
            "payment",
            "/payment/get_escrow_detail",
            ShopeeSchemaHttpMethod.Get,
            ShopeeSchemaAuthScope.Shop,
            ShopeeSchemaRequestContentKind.Query,
            [
                new SchemaParameter("optional_note", "string", false, []),
                new SchemaParameter("order_sn", "string", true, [])
            ],
            [new SchemaParameter("response", "object", false, [])]);

        var output = new ContractsWriter().WriteModule("Payment", "payment", [endpoint]);

        output.Should().Contain("public sealed record PaymentGetEscrowDetailRequest(");
        output.Should().Contain("[property: JsonPropertyName(\"order_sn\")] string OrderSn,");
        output.Should().Contain("[property: JsonPropertyName(\"optional_note\")] string? OptionalNote = null");
        output.IndexOf("OrderSn", StringComparison.Ordinal).Should().BeLessThan(output.IndexOf("OptionalNote", StringComparison.Ordinal));
    }
}
