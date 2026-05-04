namespace ShopeeSdk.Abstractions.Webhooks;

public interface IShopeeWebhookQueue
{
    Task EnqueueAsync(ShopeeWebhookEnvelope envelope, CancellationToken cancellationToken);
}
