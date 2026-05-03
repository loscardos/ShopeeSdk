using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Managers;

public sealed class ErpEndpointCoverageTests
{
    [Fact]
    public void ReferenceSchemas_ContainExpectedFullCoverageBaseline()
    {
        var endpoints = new SchemaReader().ReadDirectory(FindReferenceSchemas());

        endpoints.Should().HaveCount(425);
        endpoints.Select(endpoint => endpoint.Path).Should().Contain([
            "/product/get_category",
            "/product/get_attribute_tree",
            "/product/get_brand_list",
            "/product/add_item",
            "/product/update_item",
            "/media/upload_image",
            "/order/get_order_list",
            "/logistics/ship_order",
            "/payment/get_escrow_detail",
            "/returns/get_return_detail",
            "/merchant/get_shop_list_by_merchant",
            "/shop/get_shop_info"
        ]);
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
