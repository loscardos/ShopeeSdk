using FluentAssertions;
using Loscardos.ShopeeSdk.Tests.Generator;

namespace Loscardos.ShopeeSdk.Tests.Managers;

public sealed class ErpEndpointCoverageTests
{
    [Fact]
    public void GeneratedManagerInterfaces_ContainExpectedFullCoverageBaseline()
    {
        var managerDirectory = Path.Combine(
            SchemaFixturePaths.FindRepoRoot(),
            "src",
            "ShopeeSdk.Abstractions",
            "Managers",
            "Generated");
        var methods = Directory.EnumerateFiles(managerDirectory, "I*Api.g.cs")
            .OrderBy(path => path, StringComparer.Ordinal)
            .SelectMany(File.ReadLines)
            .Where(line => line.Contains("Async(", StringComparison.Ordinal))
            .Select(line => line.Split("Async(", StringSplitOptions.None)[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() + "Async")
            .ToArray();

        methods.Should().HaveCount(425);
        methods.Should().Contain([
            "GetCategoryAsync",
            "GetAttributeTreeAsync",
            "GetBrandListAsync",
            "AddItemAsync",
            "UpdateItemAsync",
            "UploadImageAsync",
            "GetOrderListAsync",
            "ShipOrderAsync",
            "GetEscrowDetailAsync",
            "GetReturnDetailAsync",
            "GetShopListByMerchantAsync",
            "GetShopInfoAsync"
        ]);
    }
}
