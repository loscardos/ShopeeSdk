namespace Loscardos.ShopeeSdk.Abstractions.Webhooks;

public interface IShopeeWebhookQueue
{
    Task EnqueueAsync(ShopeeWebhookEnvelope envelope, CancellationToken cancellationToken);
}
