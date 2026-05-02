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
