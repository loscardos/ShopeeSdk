namespace Loscardos.ShopeeSdk.Abstractions.Webhooks;

public interface IShopeeWebhookIdempotencyKeyFactory
{
    string Create(string rawBody, ShopeeWebhookEvent webhookEvent);
}
