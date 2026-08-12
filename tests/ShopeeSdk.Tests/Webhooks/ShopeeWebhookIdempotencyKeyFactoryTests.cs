using FluentAssertions;
using Loscardos.ShopeeSdk.Abstractions.Webhooks;
using Loscardos.ShopeeSdk.Core.Webhooks;

namespace Loscardos.ShopeeSdk.Tests.Webhooks;

public sealed class ShopeeWebhookIdempotencyKeyFactoryTests
{
    [Fact]
    public void Create_ReturnsStableKeyForSamePayload()
    {
        var factory = new ShopeeWebhookIdempotencyKeyFactory();
        const string rawBody = """{"shop_id":123,"code":3,"timestamp":1700000000,"data":{"ordersn":"A"}}""";
        var webhookEvent = new ShopeeWebhookEvent(123, null, 3, default, 1_700_000_000);

        var first = factory.Create(rawBody, webhookEvent);
        var second = factory.Create(rawBody, webhookEvent);

        first.Should().Be(second);
        first.Should().StartWith("shopee:shop:123:code:3:timestamp:1700000000:");
    }

    [Fact]
    public void Create_IncludesMerchantWhenMerchantIdIsPresent()
    {
        var factory = new ShopeeWebhookIdempotencyKeyFactory();
        const string rawBody = """{"merchant_id":456,"code":1,"timestamp":1700000000,"data":{}}""";
        var webhookEvent = new ShopeeWebhookEvent(null, 456, 1, default, 1_700_000_000);

        factory.Create(rawBody, webhookEvent)
            .Should().StartWith("shopee:merchant:456:code:1:timestamp:1700000000:");
    }
}
