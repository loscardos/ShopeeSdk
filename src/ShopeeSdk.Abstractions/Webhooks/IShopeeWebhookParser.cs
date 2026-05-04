namespace ShopeeSdk.Abstractions.Webhooks;

public interface IShopeeWebhookParser
{
    ShopeeWebhookReceiveResult TryReceive(string rawBody, string signature, DateTimeOffset receivedAt);

    object ParseData(ShopeeWebhookEvent webhookEvent);

    TData ParseData<TData>(ShopeeWebhookEvent webhookEvent);
}
