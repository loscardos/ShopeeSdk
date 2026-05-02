using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Errors;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Crypto;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.Time;

namespace ShopeeSdk.Core.Auth;

public sealed class ShopeeAuthApi(
    IShopeeClient client,
    IOptions<ShopeeOptions> options,
    IShopeeClock clock,
    IShopeeTokenStore tokenStore) : IAuthApi
{
    private readonly ShopeeOptions _options = options.Value;

    public Uri BuildAuthorizationUrl(Uri redirectUri)
    {
        var baseUrl = _options.ResolveBaseUrl();
        const string path = "/shop/auth_partner";
        var apiPath = baseUrl.AbsolutePath.TrimEnd('/') + path;
        var timestamp = clock.UnixTimeSeconds;
        var sign = ShopeeSigner.Sign(_options.PartnerKey, [_options.PartnerId.ToString(), apiPath, timestamp.ToString()]);
        var query = ShopeeQueryString.Build(new Dictionary<string, object?>
        {
            ["partner_id"] = _options.PartnerId,
            ["timestamp"] = timestamp,
            ["redirect"] = redirectUri.ToString(),
            ["sign"] = sign
        });

        return new Uri(baseUrl, $"shop/auth_partner?{query}");
    }

    public async Task<ShopeeToken> ExchangeCodeAsync(string code, long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>
        {
            ["code"] = code,
            ["partner_id"] = _options.PartnerId,
            ["shop_id"] = shopId,
            ["main_account_id"] = merchantId
        };

        var envelope = await client.SendAsync<AuthTokenResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/auth/token/get", new Dictionary<string, object?>(), body, null),
            cancellationToken);

        var response = envelope.Response ?? throw new ShopeeSdkException("Shopee auth response was empty.");
        var token = ToToken(response, shopId, merchantId, envelope.RequestId);
        await tokenStore.StoreAsync(token, cancellationToken);
        return token;
    }

    public async Task<ShopeeToken> RefreshAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        var existing = await tokenStore.GetAsync(shopId, merchantId, cancellationToken)
            ?? throw new ShopeeSdkException("No Shopee token exists for the requested shop or merchant.");

        var body = new Dictionary<string, object?>
        {
            ["partner_id"] = _options.PartnerId,
            ["refresh_token"] = existing.RefreshToken,
            ["shop_id"] = shopId,
            ["merchant_id"] = merchantId
        };

        var envelope = await client.SendAsync<AuthTokenResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/auth/access_token/get", new Dictionary<string, object?>(), body, null),
            cancellationToken);

        var response = envelope.Response ?? throw new ShopeeSdkException("Shopee refresh response was empty.");
        var token = ToToken(response, shopId, merchantId, envelope.RequestId);
        await tokenStore.StoreAsync(token, cancellationToken);
        return token;
    }

    private ShopeeToken ToToken(AuthTokenResponse response, long? shopId, long? merchantId, string? requestId)
    {
        return new ShopeeToken(
            shopId,
            merchantId,
            response.AccessToken,
            response.RefreshToken,
            clock.UtcNow.AddSeconds(response.ExpireIn),
            requestId);
    }

    private sealed record AuthTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expire_in")] int ExpireIn);
}
