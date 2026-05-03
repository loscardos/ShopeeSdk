using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class GeneratedAuthScopeTests
{
    [Fact]
    public void PublicEndpoints_DoNotRequireOwnerParameter()
    {
        var endpoints = ReadEndpoints();

        endpoints.Single(endpoint => endpoint.Path == "/public/get_access_token").AuthScope.Should().Be(ShopeeSchemaAuthScope.Public);
        endpoints.Single(endpoint => endpoint.Path == "/media/upload_image").AuthScope.Should().Be(ShopeeSchemaAuthScope.Public);
    }

    [Fact]
    public void ShopEndpoints_RequireShopOwnerParameter()
    {
        var endpoints = ReadEndpoints();

        endpoints.Single(endpoint => endpoint.Path == "/product/get_category").AuthScope.Should().Be(ShopeeSchemaAuthScope.Shop);
        endpoints.Single(endpoint => endpoint.Path == "/order/get_order_list").AuthScope.Should().Be(ShopeeSchemaAuthScope.Shop);
        endpoints.Single(endpoint => endpoint.Path == "/logistics/ship_order").AuthScope.Should().Be(ShopeeSchemaAuthScope.Shop);
    }

    [Fact]
    public void MerchantEndpoints_RequireMerchantOwnerParameter()
    {
        var endpoints = ReadEndpoints();

        endpoints.Single(endpoint => endpoint.Path == "/merchant/get_shop_list_by_merchant").AuthScope.Should().Be(ShopeeSchemaAuthScope.Merchant);
    }

    [Fact]
    public void GeneratedManagers_UseExpectedOwnerParameters()
    {
        var productApi = File.ReadAllText(Path.Combine(FindRepoRoot(), "src", "ShopeeSdk.Abstractions", "Managers", "Generated", "IProductApi.g.cs"));
        var publicApi = File.ReadAllText(Path.Combine(FindRepoRoot(), "src", "ShopeeSdk.Abstractions", "Managers", "Generated", "IPublicApi.g.cs"));
        var merchantApi = File.ReadAllText(Path.Combine(FindRepoRoot(), "src", "ShopeeSdk.Abstractions", "Managers", "Generated", "IMerchantApi.g.cs"));

        productApi.Should().Contain("GetCategoryAsync(long shopId, ProductGetCategoryRequest request, CancellationToken cancellationToken)");
        publicApi.Should().Contain("GetAccessTokenAsync(PublicGetAccessTokenRequest request, CancellationToken cancellationToken)");
        merchantApi.Should().Contain("GetShopListByMerchantAsync(long merchantId, MerchantGetShopListByMerchantRequest request, CancellationToken cancellationToken)");
    }

    private static IReadOnlyList<SchemaEndpoint> ReadEndpoints()
    {
        return new SchemaReader().ReadDirectory(FindReferenceSchemas());
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

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ShopeeSdk.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root from test output directory.");
    }
}
