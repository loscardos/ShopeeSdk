using ShopeeSdk.Abstractions.Webhooks;

namespace ShopeeSdk.Core.Webhooks;

public sealed class ShopeeWebhookTimestampValidator(ShopeeWebhookOptions options)
{
    public bool IsFresh(ShopeeWebhookEvent webhookEvent, DateTimeOffset receivedAt)
    {
        if (!options.RequireFreshTimestamp)
        {
            return true;
        }

        if (webhookEvent.Timestamp is null)
        {
            return false;
        }

        var sentAt = DateTimeOffset.FromUnixTimeSeconds(webhookEvent.Timestamp.Value);
        var age = (receivedAt - sentAt).Duration();
        return age <= options.TimestampTolerance;
    }
}
