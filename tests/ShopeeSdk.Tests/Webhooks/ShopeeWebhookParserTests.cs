using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Core.Webhooks;
using ShopeeSdk.Generated.Webhooks;

namespace ShopeeSdk.Tests.Webhooks;

public sealed class ShopeeWebhookParserTests
{
    [Fact]
    public void TryReceive_ReturnsAcceptedEnvelopeForValidSignatureAndFreshTimestamp()
    {
        const string rawBody = """{"shop_id":1274495,"code":3,"timestamp":1700000000,"data":{"ordersn":"220810QSK8S7BX","status":"PROCESSED","update_time":1700000000}}""";
        var signature = Sign(rawBody);
        var parser = CreateParser();

        var result = parser.TryReceive(rawBody, signature, DateTimeOffset.FromUnixTimeSeconds(1_700_000_060));

        result.Accepted.Should().BeTrue();
        result.Envelope!.IdempotencyKey.Should().StartWith("shopee:shop:1274495:code:3:timestamp:1700000000:");
    }

    [Fact]
    public void TryReceive_ReturnsRejectedEnvelopeForStaleTimestamp()
    {
        const string rawBody = """{"shop_id":1274495,"code":3,"timestamp":1700000000,"data":{"ordersn":"220810QSK8S7BX","status":"PROCESSED","update_time":1700000000}}""";
        var parser = CreateParser();

        var result = parser.TryReceive(rawBody, Sign(rawBody), DateTimeOffset.FromUnixTimeSeconds(1_700_001_000));

        result.Accepted.Should().BeFalse();
        result.Reason.Should().Be("stale_timestamp");
    }

    [Fact]
    public void ParseData_ReturnsTypedPayloadForKnownCode()
    {
        const string rawBody = """{"shop_id":1274495,"code":3,"timestamp":1700000000,"data":{"ordersn":"220810QSK8S7BX","status":"PROCESSED","update_time":1700000000}}""";
        var parser = CreateParser();
        var result = parser.TryReceive(rawBody, Sign(rawBody), DateTimeOffset.FromUnixTimeSeconds(1_700_000_060));

        var payload = parser.ParseData(result.Envelope!.Event);

        payload.Should().BeOfType<OrderStatusPushData>()
            .Which.OrderSn.Should().Be("220810QSK8S7BX");
    }

    [Fact]
    public void TryReceive_ParsesOfficialOrderStatusCallbackFixture()
    {
        var rawBody = File.ReadAllText(Path.Combine(
            "Fixtures",
            "Webhooks",
            "official-order-status-callback.json"));
        var parser = CreateParser();

        var result = parser.TryReceive(rawBody, Sign(rawBody), DateTimeOffset.FromUnixTimeSeconds(1_660_123_187));
        var payload = parser.ParseData(result.Envelope!.Event);

        result.Accepted.Should().BeTrue();
        result.Envelope.Event.ShopId.Should().Be(727720655);
        payload.Should().BeOfType<OrderStatusPushData>()
            .Which.CompletedScenario.Should().BeEmpty();
    }

    private static ShopeeWebhookParser CreateParser()
        => new(
            new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions { PartnerKey = "partner-key" })),
            new ShopeeWebhookTimestampValidator(new ShopeeWebhookOptions()),
            new ShopeeWebhookIdempotencyKeyFactory());

    private static string Sign(string rawBody)
        => Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("partner-key"),
            Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();
}
