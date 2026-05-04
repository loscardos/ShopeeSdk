using FluentAssertions;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Core.Webhooks;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeWebhookCallbackCanaryTests
{
    [ShopeeWebhookCallbackFact]
    public void TryReceive_AcceptsCapturedShopeeCallback()
    {
        var payloadPath = Environment.GetEnvironmentVariable("SHOPEE_WEBHOOK_PAYLOAD_PATH")!;
        var signature = Environment.GetEnvironmentVariable("SHOPEE_WEBHOOK_SIGNATURE")!;
        var partnerKey = Environment.GetEnvironmentVariable("SHOPEE_PARTNER_KEY")!;
        var receivedAt = GetReceivedAt();
        var parser = new ShopeeWebhookParser(
            new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions { PartnerKey = partnerKey })),
            new ShopeeWebhookTimestampValidator(new ShopeeWebhookOptions()),
            new ShopeeWebhookIdempotencyKeyFactory());

        var rawBody = File.ReadAllText(payloadPath);
        var result = parser.TryReceive(rawBody, signature, receivedAt);

        result.Accepted.Should().BeTrue(result.Reason);
        result.Envelope!.RawBody.Should().Be(rawBody);
        parser.ParseData(result.Envelope.Event).Should().NotBeNull();
    }

    private static DateTimeOffset GetReceivedAt()
    {
        var receivedAtUnix = Environment.GetEnvironmentVariable("SHOPEE_WEBHOOK_RECEIVED_AT_UNIX");
        if (long.TryParse(receivedAtUnix, out var unixSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }

        return DateTimeOffset.UtcNow;
    }
}
