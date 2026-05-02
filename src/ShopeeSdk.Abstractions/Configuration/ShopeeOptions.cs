using ShopeeSdk.Abstractions.Errors;

namespace ShopeeSdk.Abstractions.Configuration;

public sealed class ShopeeOptions
{
    public long PartnerId { get; set; }
    public string PartnerKey { get; set; } = string.Empty;
    public ShopeeRegion Region { get; set; } = ShopeeRegion.Global;
    public Uri? BaseUrl { get; set; }
    public TimeSpan TokenRefreshSkew { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxTransientRetries { get; set; } = 2;
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(250);
    public string UserAgent { get; set; } = "InternalShopeeSdk/0.2";

    public Uri ResolveBaseUrl()
    {
        if (BaseUrl is not null)
        {
            return BaseUrl;
        }

        return Region switch
        {
            ShopeeRegion.Global => new Uri("https://partner.shopeemobile.com/api/v2/"),
            ShopeeRegion.China => new Uri("https://openplatform.shopee.cn/api/v2/public/"),
            ShopeeRegion.Brazil => new Uri("https://openplatform.shopee.com.br/api/v2/"),
            ShopeeRegion.TestGlobal => new Uri("https://partner.test-stable.shopeemobile.com/api/v2/"),
            ShopeeRegion.TestChina => new Uri("https://openplatform.test-stable.shopee.cn/api/v2/"),
            _ => throw new ShopeeSdkException($"Unsupported Shopee region: {Region}")
        };
    }
}
