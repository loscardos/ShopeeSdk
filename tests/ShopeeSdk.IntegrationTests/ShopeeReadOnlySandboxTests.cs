using FluentAssertions;
using ShopeeSdk.Generated.Logistics;
using ShopeeSdk.Generated.Order;
using ShopeeSdk.Generated.Payment;
using ShopeeSdk.Generated.Product;
using ShopeeSdk.Generated.Shop;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeReadOnlySandboxTests
{
    [ShopeeSandboxTokenFact]
    public async Task ProductGetCategoryAsync_CallsSandboxReadOnlyEndpoint()
    {
        var fixture = new ShopeeSandboxFixture();
        await fixture.StoreConfiguredShopTokenAsync();

        var response = await fixture.CreateProductApi().GetCategoryAsync(
            ShopeeSandboxConfiguration.ShopId,
            new ProductGetCategoryRequest(Language: "en"),
            CancellationToken.None);

        response.CategoryList.Should().NotBeNull();
    }

    [ShopeeSandboxTokenFact]
    public async Task OrderGetOrderListAsync_CallsSandboxReadOnlyEndpoint()
    {
        var fixture = new ShopeeSandboxFixture();
        await fixture.StoreConfiguredShopTokenAsync();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var response = await fixture.CreateOrderApi().GetOrderListAsync(
            ShopeeSandboxConfiguration.ShopId,
            new OrderGetOrderListRequest(
                TimeRangeField: "update_time",
                TimeFrom: now - 3600,
                TimeTo: now,
                PageSize: 20),
            CancellationToken.None);

        response.OrderList.Should().NotBeNull();
    }

    [ShopeeSandboxTokenFact]
    public async Task ShopGetShopInfoAsync_CallsSandboxReadOnlyEndpoint()
    {
        var fixture = new ShopeeSandboxFixture();
        await fixture.StoreConfiguredShopTokenAsync();

        var response = await fixture.CreateShopApi().GetShopInfoAsync(
            ShopeeSandboxConfiguration.ShopId,
            new ShopGetShopInfoRequest(),
            CancellationToken.None);

        response.Should().NotBeNull();
    }

    [ShopeeSandboxOrderFact]
    public async Task OrderGetOrderDetailAsync_CallsSandboxReadOnlyEndpoint()
    {
        var fixture = new ShopeeSandboxFixture();
        await fixture.StoreConfiguredShopTokenAsync();

        var response = await fixture.CreateOrderApi().GetOrderDetailAsync(
            ShopeeSandboxConfiguration.ShopId,
            new OrderGetOrderDetailRequest(
                OrderSnList: ShopeeSandboxConfiguration.OrderSn,
                ResponseOptionalFields: "order_status,total_amount,update_time"),
            CancellationToken.None);

        response.OrderList.Should().NotBeNull();
    }

    [ShopeeSandboxOrderFact]
    public async Task PaymentGetEscrowDetailAsync_CallsSandboxReadOnlyEndpoint()
    {
        var fixture = new ShopeeSandboxFixture();
        await fixture.StoreConfiguredShopTokenAsync();

        var response = await fixture.CreatePaymentApi().GetEscrowDetailAsync(
            ShopeeSandboxConfiguration.ShopId,
            new PaymentGetEscrowDetailRequest(OrderSn: ShopeeSandboxConfiguration.OrderSn),
            CancellationToken.None);

        response.Should().NotBeNull();
    }

    [ShopeeSandboxOrderFact]
    public async Task LogisticsGetTrackingInfoAsync_CallsSandboxReadOnlyEndpoint()
    {
        var packageNumber = ShopeeSandboxConfiguration.PackageNumber;
        if (string.IsNullOrWhiteSpace(packageNumber))
        {
            return;
        }

        var fixture = new ShopeeSandboxFixture();
        await fixture.StoreConfiguredShopTokenAsync();

        var response = await fixture.CreateLogisticsApi().GetTrackingInfoAsync(
            ShopeeSandboxConfiguration.ShopId,
            new LogisticsGetTrackingInfoRequest(
                OrderSn: ShopeeSandboxConfiguration.OrderSn,
                PackageNumber: packageNumber),
            CancellationToken.None);

        response.Should().NotBeNull();
    }
}
