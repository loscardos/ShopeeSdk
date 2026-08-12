using Loscardos.ShopeeSdk.Abstractions.Auth;
using Loscardos.ShopeeSdk.Abstractions.Configuration;

namespace Loscardos.ShopeeSdk.IntegrationTests;

public static class ShopeeSandboxConfiguration
{
    public static ShopeeOptions CreateOptions()
    {
        return new ShopeeOptions
        {
            PartnerId = long.Parse(Required("SHOPEE_PARTNER_ID")),
            PartnerKey = Required("SHOPEE_PARTNER_KEY"),
            Region = Enum.TryParse<ShopeeRegion>(Environment.GetEnvironmentVariable("SHOPEE_REGION"), true, out var region)
                ? region
                : ShopeeRegion.TestGlobal,
            MaxTransientRetries = 1,
            RetryBaseDelay = TimeSpan.FromMilliseconds(100)
        };
    }

    public static long ShopId => long.Parse(Required("SHOPEE_SHOP_ID"));
    public static string AuthCode => Required("SHOPEE_AUTH_CODE");
    public static string OrderSn => Required("SHOPEE_SANDBOX_ORDER_SN");
    public static string? PackageNumber => Environment.GetEnvironmentVariable("SHOPEE_SANDBOX_PACKAGE_NUMBER");

    public static Uri RedirectUri
    {
        get
        {
            var redirect = Environment.GetEnvironmentVariable("SHOPEE_REDIRECT_URL");
            return string.IsNullOrWhiteSpace(redirect)
                ? new Uri("https://localhost/shopee/callback")
                : new Uri(redirect);
        }
    }

    public static ShopeeToken CreateToken()
    {
        return new ShopeeToken(
            ShopId,
            null,
            Required("SHOPEE_ACCESS_TOKEN"),
            Environment.GetEnvironmentVariable("SHOPEE_REFRESH_TOKEN") ?? string.Empty,
            DateTimeOffset.UtcNow.AddHours(1),
            "sandbox-env");
    }

    private static string Required(string name)
    {
        return Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"{name} is required.");
    }
}
