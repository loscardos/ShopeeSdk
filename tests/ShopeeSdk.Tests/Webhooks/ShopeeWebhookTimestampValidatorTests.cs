using FluentAssertions;
using ShopeeSdk.Abstractions.Webhooks;
using ShopeeSdk.Core.Webhooks;

namespace ShopeeSdk.Tests.Webhooks;

public sealed class ShopeeWebhookTimestampValidatorTests
{
    [Fact]
    public void IsFresh_ReturnsTrueWhenTimestampIsInsideTolerance()
    {
        var validator = new ShopeeWebhookTimestampValidator(new ShopeeWebhookOptions
        {
            RequireFreshTimestamp = true,
            TimestampTolerance = TimeSpan.FromMinutes(5)
        });
        var receivedAt = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);
        var webhookEvent = new ShopeeWebhookEvent(123, null, 3, default, 1_699_999_940);

        validator.IsFresh(webhookEvent, receivedAt).Should().BeTrue();
    }

    [Fact]
    public void IsFresh_ReturnsFalseWhenTimestampIsOlderThanTolerance()
    {
        var validator = new ShopeeWebhookTimestampValidator(new ShopeeWebhookOptions
        {
            RequireFreshTimestamp = true,
            TimestampTolerance = TimeSpan.FromMinutes(5)
        });
        var receivedAt = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);
        var webhookEvent = new ShopeeWebhookEvent(123, null, 3, default, 1_699_999_000);

        validator.IsFresh(webhookEvent, receivedAt).Should().BeFalse();
    }

    [Fact]
    public void IsFresh_ReturnsFalseWhenTimestampIsMissing()
    {
        var validator = new ShopeeWebhookTimestampValidator(new ShopeeWebhookOptions
        {
            RequireFreshTimestamp = true
        });
        var webhookEvent = new ShopeeWebhookEvent(123, null, 3, default, null);

        validator.IsFresh(webhookEvent, DateTimeOffset.UtcNow).Should().BeFalse();
    }
}
