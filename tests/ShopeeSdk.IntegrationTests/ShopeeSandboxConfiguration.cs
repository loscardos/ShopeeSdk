using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;

namespace ShopeeSdk.IntegrationTests;

public static class ShopeeSandboxConfiguration
{
    public static ShopeeOptions CreateOptions()
    {
        return new ShopeeOptions
        {
            PartnerId = long.Parse(Environment.GetEnvironmentVariable("SHOPEE_PARTNER_ID")!),
            PartnerKey = Environment.GetEnvironmentVariable("SHOPEE_PARTNER_KEY")!,
            Region = ShopeeRegion.TestGlobal,
            MaxTransientRetries = 1,
            RetryBaseDelay = TimeSpan.FromMilliseconds(100)
        };
    }

    public static long ShopId => long.Parse(Environment.GetEnvironmentVariable("SHOPEE_SHOP_ID")!);

    public static ShopeeToken CreateToken()
    {
        return new ShopeeToken(
            ShopId,
            null,
            Environment.GetEnvironmentVariable("SHOPEE_ACCESS_TOKEN")!,
            Environment.GetEnvironmentVariable("SHOPEE_REFRESH_TOKEN") ?? string.Empty,
            DateTimeOffset.UtcNow.AddHours(1),
            "sandbox-env");
    }
}
