using FluentAssertions;
using Loscardos.ShopeeSdk.SampleConsole;

namespace Loscardos.ShopeeSdk.Tests.SampleConsole;

public sealed class EndpointCertificationMatrixTests
{
    [Fact]
    public void Create_ClassifiesCertifiedReadonlyAndMutationEndpoints()
    {
        var rows = EndpointCertificationMatrix.Create([
            new GeneratedEndpoint("Product", "GET", "/product/get_category", "Shop"),
            new GeneratedEndpoint("Product", "POST", "/product/add_item", "Shop"),
            new GeneratedEndpoint("Order", "GET", "/order/get_order_detail", "Shop")
        ], new HashSet<string>(StringComparer.Ordinal)
        {
            ReadonlyCertificationCatalog.Key("GET", "/product/get_category")
        });

        rows.Should().Contain(row =>
            row.Path == "/product/get_category"
            && row.Classification == EndpointClassification.Readonly
            && row.Status == EndpointCertificationStatus.ReadonlyCertified);
        rows.Should().Contain(row =>
            row.Path == "/product/add_item"
            && row.Classification == EndpointClassification.Mutation
            && row.Status == EndpointCertificationStatus.MutationNeedsFixture);
        rows.Should().Contain(row =>
            row.Path == "/order/get_order_detail"
            && row.Status == EndpointCertificationStatus.ReadonlyNeedsLiveCertification);
    }

    [Fact]
    public void GeneratedEndpointInventory_ReadFromDirectory_ReadsGeneratedShopeeManagers()
    {
        var endpoints = GeneratedEndpointInventory.ReadFromDirectory(GeneratedEndpointInventory.DefaultGeneratedManagersDirectory);

        endpoints.Should().Contain(endpoint =>
            endpoint.Area == "Product"
            && endpoint.Method == "GET"
            && endpoint.Path == "/product/get_category"
            && endpoint.AuthScope == "Shop");
        endpoints.Should().Contain(endpoint =>
            endpoint.Area == "Product"
            && endpoint.Method == "POST"
            && endpoint.Path == "/product/add_item"
            && endpoint.AuthScope == "Shop");
    }
}
