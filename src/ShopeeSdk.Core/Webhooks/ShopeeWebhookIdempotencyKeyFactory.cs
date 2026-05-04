using System.Security.Cryptography;
using System.Text;
using ShopeeSdk.Abstractions.Webhooks;

namespace ShopeeSdk.Core.Webhooks;

public sealed class ShopeeWebhookIdempotencyKeyFactory : IShopeeWebhookIdempotencyKeyFactory
{
    public string Create(string rawBody, ShopeeWebhookEvent webhookEvent)
    {
        var owner = webhookEvent.ShopId is not null
            ? $"shop:{webhookEvent.ShopId.Value}"
            : webhookEvent.MerchantId is not null
                ? $"merchant:{webhookEvent.MerchantId.Value}"
                : "owner:none";
        var timestamp = webhookEvent.Timestamp?.ToString() ?? "none";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();
        return $"shopee:{owner}:code:{webhookEvent.Code}:timestamp:{timestamp}:{hash}";
    }
}
