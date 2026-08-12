using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Abstractions.Errors;
using Loscardos.ShopeeSdk.Abstractions.Webhooks;

namespace Loscardos.ShopeeSdk.Core.Webhooks;

public sealed class ShopeeWebhookVerifier(IOptions<ShopeeOptions> options) : IShopeeWebhookVerifier
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ShopeeOptions _options = options.Value;

    public bool Verify(string rawBody, string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var pushPartnerKey = string.IsNullOrWhiteSpace(_options.PushPartnerKey)
            ? _options.PartnerKey
            : _options.PushPartnerKey;

        var signatureBase = string.IsNullOrWhiteSpace(_options.PushCallbackUrl)
            ? rawBody
            : $"{_options.PushCallbackUrl}|{rawBody}";
        var expected = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(pushPartnerKey),
            Encoding.UTF8.GetBytes(signatureBase))).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    public ShopeeWebhookEvent ParseVerified(string rawBody, string signature)
    {
        if (!Verify(rawBody, signature))
        {
            throw new ShopeeSdkException("Invalid Shopee webhook signature.");
        }

        return JsonSerializer.Deserialize<ShopeeWebhookEvent>(rawBody, JsonOptions)
            ?? throw new ShopeeSdkException("Shopee webhook payload was empty.");
    }
}
