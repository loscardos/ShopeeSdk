using Xunit;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeSandboxFactAttribute : FactAttribute
{
    public ShopeeSandboxFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOPEE_PARTNER_ID"))
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOPEE_PARTNER_KEY")))
        {
            Skip = "Shopee sandbox credentials are not configured.";
        }
    }
}

public sealed class ShopeeSandboxTokenFactAttribute : FactAttribute
{
    public ShopeeSandboxTokenFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOPEE_PARTNER_ID"))
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOPEE_PARTNER_KEY"))
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOPEE_SHOP_ID"))
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOPEE_ACCESS_TOKEN")))
        {
            Skip = "Shopee sandbox shop token credentials are not configured.";
        }
    }
}
