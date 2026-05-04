using System.Text.Json;
using FluentAssertions;
using ShopeeSdk.Generated.Webhooks;

namespace ShopeeSdk.Tests.Webhooks;

public sealed class ShopeeWebhookTypedPayloadTests
{
    [Fact]
    public void ReservedStockChangePushPayload_DeserializesCapturedExample()
    {
        var payload = JsonSerializer.Deserialize<ReservedStockChangePushData>("""
        {
          "shop_id": 1274495,
          "item_id": 18614185187,
          "variation_id": 87912785718,
          "changed_values": [
            { "name": "reserved_stock", "old": 4951, "new": 4950 }
          ],
          "promotion_type": "flash_sale",
          "promotion_id": 104993304719361,
          "action": "place_order",
          "ordersn": "220810QXVJM3EX",
          "update_time": 1660124246
        }
        """, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        payload!.ItemId.Should().Be(18614185187);
        payload.ChangedValues.Should().ContainSingle()
            .Which.Name.Should().Be("reserved_stock");
    }

    [Fact]
    public void OrderStatusPushPayload_DeserializesCapturedExample()
    {
        var payload = JsonSerializer.Deserialize<OrderStatusPushData>("""
        {
          "items": [],
          "ordersn": "220810QSK8S7BX",
          "status": "PROCESSED",
          "completed_scenario": "NORMAL",
          "update_time": 1660123127
        }
        """, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        payload!.OrderSn.Should().Be("220810QSK8S7BX");
        payload.Status.Should().Be("PROCESSED");
        payload.CompletedScenario.Should().Be("NORMAL");
    }
}
