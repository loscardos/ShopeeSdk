using FluentAssertions;
using ShopeeSdk.Generated.Webhooks;

namespace ShopeeSdk.Tests.Webhooks;

public sealed class ShopeeWebhookCatalogTests
{
    [Fact]
    public void All_ContainsEveryCapturedPushMechanism()
    {
        ShopeeWebhookCatalog.All.Should().HaveCount(34);
        ShopeeWebhookCatalog.All.Select(item => item.Name).Should().Contain([
            "reserved_stock_change_push",
            "order_status_push",
            "return_updates_push",
            "fbs_br_invoice_issued_push"
        ]);
    }

    [Fact]
    public void FindByCode_ReturnsOrderStatusPush()
    {
        var item = ShopeeWebhookCatalog.FindByCode(ShopeeWebhookEventCode.OrderStatusPush);

        item.Should().NotBeNull();
        item!.PushApiId.Should().Be(1);
        item.Name.Should().Be("order_status_push");
        item.CategoryName.Should().Be("Order Push");
        item.RetrySeconds.Should().Equal(300, 1800, 10800);
    }
}
