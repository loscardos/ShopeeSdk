namespace ShopeeSdk.Abstractions.Webhooks;

public interface IShopeeWebhookVerifier
{
    bool Verify(string rawBody, string signature);
    ShopeeWebhookEvent ParseVerified(string rawBody, string signature);
}
