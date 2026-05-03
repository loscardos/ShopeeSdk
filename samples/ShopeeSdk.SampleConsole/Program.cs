using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Errors;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.Managers;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Core.Time;
using ShopeeSdk.Generated.Order;
using ShopeeSdk.Generated.Product;
using ShopeeSdk.Generated.Shop;
using GeneratedProductApi = ShopeeSdk.Core.Managers.Generated.ProductApi;
using GeneratedShopApi = ShopeeSdk.Core.Managers.Generated.ShopApi;

static string Required(string name)
{
    return Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException($"{name} is required.");
}

var options = Options.Create(new ShopeeOptions
{
    PartnerId = long.Parse(Environment.GetEnvironmentVariable("SHOPEE_PARTNER_ID") ?? "1"),
    PartnerKey = Environment.GetEnvironmentVariable("SHOPEE_PARTNER_KEY") ?? "key",
    Region = Enum.TryParse<ShopeeRegion>(Environment.GetEnvironmentVariable("SHOPEE_REGION"), true, out var region)
        ? region
        : ShopeeRegion.TestGlobal,
    MaxTransientRetries = 1
});

var redirectUrl = Environment.GetEnvironmentVariable("SHOPEE_REDIRECT_URL");
var tokenStore = new SampleTokenStore();
var client = new ShopeeClient(
    new HttpClient { BaseAddress = options.Value.ResolveBaseUrl() },
    options,
    new SystemShopeeClock(),
    new NoopShopeeRateLimiter(),
    NullLogger<ShopeeClient>.Instance);
var authApi = new ShopeeAuthApi(client, options, new SystemShopeeClock(), tokenStore);

if (args is ["auth-url"])
{
    options.Value.PartnerId = long.Parse(Required("SHOPEE_PARTNER_ID"));
    options.Value.PartnerKey = Required("SHOPEE_PARTNER_KEY");

    if (string.IsNullOrWhiteSpace(redirectUrl))
    {
        throw new InvalidOperationException("SHOPEE_REDIRECT_URL is required for auth-url.");
    }

    Console.WriteLine(authApi.BuildAuthorizationUrl(new Uri(redirectUrl)));
    return;
}

if (args is ["exchange-code"])
{
    options.Value.PartnerId = long.Parse(Required("SHOPEE_PARTNER_ID"));
    options.Value.PartnerKey = Required("SHOPEE_PARTNER_KEY");

    var code = Required("SHOPEE_AUTH_CODE");
    var shopId = long.Parse(Required("SHOPEE_SHOP_ID"));
    var token = await authApi.ExchangeCodeAsync(code, shopId, null, CancellationToken.None);
    Console.WriteLine($"shop_id={token.ShopId}");
    Console.WriteLine($"access_token_length={token.AccessToken.Length}");
    Console.WriteLine($"refresh_token_length={token.RefreshToken.Length}");
    Console.WriteLine($"expires_at={token.ExpiresAt:O}");
    return;
}

if (args is ["get-orders"])
{
    options.Value.PartnerId = long.Parse(Required("SHOPEE_PARTNER_ID"));
    options.Value.PartnerKey = Required("SHOPEE_PARTNER_KEY");

    var shopId = long.Parse(Required("SHOPEE_SHOP_ID"));
    await tokenStore.StoreAsync(
        new ShopeeToken(
            shopId,
            null,
            Required("SHOPEE_ACCESS_TOKEN"),
            Environment.GetEnvironmentVariable("SHOPEE_REFRESH_TOKEN") ?? string.Empty,
            DateTimeOffset.UtcNow.AddHours(1),
            "sample-env"),
        CancellationToken.None);

    var tokenService = new ShopeeTokenService(tokenStore, new MissingAuthApi(), options, new SystemShopeeClock());
    var orderApi = new OrderApi(client, tokenService);
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var response = await orderApi.GetOrderListAsync(
        shopId,
        new GetOrderListRequest("update_time", now - 3600, now, 20),
        CancellationToken.None);

    Console.WriteLine($"orders={response.OrderList.Count}");
    Console.WriteLine($"more={response.More}");
    return;
}

if (args is ["sandbox-check"])
{
    options.Value.PartnerId = long.Parse(Required("SHOPEE_PARTNER_ID"));
    options.Value.PartnerKey = Required("SHOPEE_PARTNER_KEY");

    var shopId = long.Parse(Required("SHOPEE_SHOP_ID"));
    await tokenStore.StoreAsync(
        new ShopeeToken(
            shopId,
            null,
            Required("SHOPEE_ACCESS_TOKEN"),
            Environment.GetEnvironmentVariable("SHOPEE_REFRESH_TOKEN") ?? string.Empty,
            DateTimeOffset.UtcNow.AddHours(1),
            "sample-env"),
        CancellationToken.None);

    var tokenService = new ShopeeTokenService(tokenStore, new MissingAuthApi(), options, new SystemShopeeClock());
    var productApi = new GeneratedProductApi(client, tokenService);
    var shopApi = new GeneratedShopApi(client, tokenService);

    var categories = await productApi.GetCategoryAsync(
        shopId,
        new ProductGetCategoryRequest(Language: "en"),
        CancellationToken.None);

    var shopInfo = await shopApi.GetShopInfoAsync(
        shopId,
        new ShopGetShopInfoRequest(),
        CancellationToken.None);

    Console.WriteLine($"categories={categories.CategoryList?.Count ?? 0}");
    Console.WriteLine($"shop_info_received={shopInfo is not null}");
    return;
}

Console.WriteLine("Usage:");
Console.WriteLine("  dotnet run --project samples/ShopeeSdk.SampleConsole -- auth-url");
Console.WriteLine("  dotnet run --project samples/ShopeeSdk.SampleConsole -- exchange-code");
Console.WriteLine("  dotnet run --project samples/ShopeeSdk.SampleConsole -- get-orders");
Console.WriteLine("  dotnet run --project samples/ShopeeSdk.SampleConsole -- sandbox-check");

internal sealed class SampleTokenStore : IShopeeTokenStore
{
    private readonly Dictionary<string, ShopeeToken> _tokens = [];

    public Task<ShopeeToken?> GetAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        _tokens.TryGetValue(Key(shopId, merchantId), out var token);
        return Task.FromResult(token);
    }

    public Task StoreAsync(ShopeeToken token, CancellationToken cancellationToken)
    {
        _tokens[Key(token.ShopId, token.MerchantId)] = token;
        return Task.CompletedTask;
    }

    public Task ClearAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        _tokens.Remove(Key(shopId, merchantId));
        return Task.CompletedTask;
    }

    private static string Key(long? shopId, long? merchantId) => $"{shopId?.ToString() ?? "none"}:{merchantId?.ToString() ?? "none"}";
}

internal sealed class MissingAuthApi : IAuthApi
{
    public Uri BuildAuthorizationUrl(Uri redirectUri)
    {
        throw new ShopeeSdkException("Sample refresh is not configured.");
    }

    public Task<ShopeeToken> ExchangeCodeAsync(string code, long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        throw new ShopeeSdkException("Sample refresh is not configured.");
    }

    public Task<ShopeeToken> RefreshAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        throw new ShopeeSdkException("Sample refresh is not configured.");
    }
}
