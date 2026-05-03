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
        endpoints.Single(endpoint => endpoint.Path == "/product/get_category").RequestContentKind.Should().Be(ShopeeSchemaRequestContentKind.Query);
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
        var productApi = File.ReadAllText(Path.Combine(SchemaFixturePaths.FindRepoRoot(), "src", "ShopeeSdk.Abstractions", "Managers", "Generated", "IProductApi.g.cs"));
        var publicApi = File.ReadAllText(Path.Combine(SchemaFixturePaths.FindRepoRoot(), "src", "ShopeeSdk.Abstractions", "Managers", "Generated", "IPublicApi.g.cs"));
        var merchantApi = File.ReadAllText(Path.Combine(SchemaFixturePaths.FindRepoRoot(), "src", "ShopeeSdk.Abstractions", "Managers", "Generated", "IMerchantApi.g.cs"));

        productApi.Should().Contain("GetCategoryAsync(long shopId, ProductGetCategoryRequest request, CancellationToken cancellationToken)");
        publicApi.Should().Contain("GetAccessTokenAsync(PublicGetAccessTokenRequest request, CancellationToken cancellationToken)");
        merchantApi.Should().Contain("GetShopListByMerchantAsync(long merchantId, MerchantGetShopListByMerchantRequest request, CancellationToken cancellationToken)");
    }

    private static IReadOnlyList<SchemaEndpoint> ReadEndpoints()
    {
        return new SchemaReader().ReadDirectory(SchemaFixturePaths.FindSchemas());
    }
}
