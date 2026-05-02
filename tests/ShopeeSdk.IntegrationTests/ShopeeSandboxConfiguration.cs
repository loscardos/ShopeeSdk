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
            MaxTransientRetries = 1
        };
    }
}
