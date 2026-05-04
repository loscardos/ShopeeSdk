namespace ShopeeSdk.Abstractions.Webhooks;

public interface IShopeeWebhookRawPayloadSink
{
    Task SaveAsync(ShopeeWebhookEnvelope envelope, CancellationToken cancellationToken);
}
