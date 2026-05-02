using FluentAssertions;
using ShopeeSdk.Abstractions.Managers;

namespace ShopeeSdk.Tests.Managers;

public sealed class ErpEndpointCoverageTests
{
    [Fact]
    public void ProductApi_ContainsMinimumProductionPilotMethods()
    {
        typeof(IProductApi).GetMethods().Select(method => method.Name).Should().BeEquivalentTo([
            "GetItemListAsync",
            "GetItemBaseInfoAsync",
            "GetModelListAsync",
            "UpdateStockAsync",
            "UpdatePriceAsync"
        ]);
    }

    [Fact]
    public void OrderApi_ContainsMinimumProductionPilotMethods()
    {
        typeof(IOrderApi).GetMethods().Select(method => method.Name).Should().BeEquivalentTo([
            "GetOrderListAsync",
            "GetOrderDetailAsync",
            "GetShipmentListAsync",
            "SetNoteAsync"
        ]);
    }

    [Fact]
    public void LogisticsApi_ContainsMinimumProductionPilotMethods()
    {
        typeof(ILogisticsApi).GetMethods().Select(method => method.Name).Should().BeEquivalentTo([
            "GetShippingParameterAsync",
            "ShipOrderAsync",
            "GetTrackingInfoAsync",
            "GetShippingDocumentParameterAsync",
            "CreateShippingDocumentAsync",
            "DownloadShippingDocumentAsync"
        ]);
    }
}
