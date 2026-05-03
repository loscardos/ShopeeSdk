using Xunit;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeSandboxFactAttribute : FactAttribute
{
    public ShopeeSandboxFactAttribute()
    {
        Skip = ShopeeSandboxSkip.BaseCredentials();
    }
}

public sealed class ShopeeSandboxTokenFactAttribute : FactAttribute
{
    public ShopeeSandboxTokenFactAttribute()
    {
        Skip = ShopeeSandboxSkip.TokenCredentials();
    }
}

public sealed class ShopeeSandboxRefreshTokenFactAttribute : FactAttribute
{
    public ShopeeSandboxRefreshTokenFactAttribute()
    {
        Skip = ShopeeSandboxSkip.RefreshTokenCredentials();
    }
}

public sealed class ShopeeSandboxAuthCodeFactAttribute : FactAttribute
{
    public ShopeeSandboxAuthCodeFactAttribute()
    {
        Skip = ShopeeSandboxSkip.AuthCodeCredentials();
    }
}

public sealed class ShopeeSandboxOrderFactAttribute : FactAttribute
{
    public ShopeeSandboxOrderFactAttribute()
    {
        Skip = ShopeeSandboxSkip.OrderCredentials();
    }
}

internal static class ShopeeSandboxSkip
{
    public static string? BaseCredentials()
    {
        if (!IsEnabled())
        {
            return "Set SHOPEE_RUN_SANDBOX=true to run Shopee sandbox integration tests.";
        }

        if (Missing("SHOPEE_PARTNER_ID") || Missing("SHOPEE_PARTNER_KEY"))
        {
            return "Shopee sandbox partner credentials are not configured.";
        }

        return null;
    }

    public static string? TokenCredentials()
    {
        return BaseCredentials()
            ?? (Missing("SHOPEE_SHOP_ID") || Missing("SHOPEE_ACCESS_TOKEN")
                ? "Shopee sandbox shop token credentials are not configured."
                : null);
    }

    public static string? RefreshTokenCredentials()
    {
        return TokenCredentials()
            ?? (Missing("SHOPEE_REFRESH_TOKEN")
                ? "Shopee sandbox refresh token is not configured."
                : null);
    }

    public static string? AuthCodeCredentials()
    {
        return BaseCredentials()
            ?? (Missing("SHOPEE_AUTH_CODE") || Missing("SHOPEE_SHOP_ID")
                ? "Shopee sandbox auth code or shop id is not configured."
                : null);
    }

    public static string? OrderCredentials()
    {
        return TokenCredentials()
            ?? (Missing("SHOPEE_SANDBOX_ORDER_SN")
                ? "SHOPEE_SANDBOX_ORDER_SN is not configured for order-specific read-only checks."
                : null);
    }

    private static bool IsEnabled()
    {
        return string.Equals(Environment.GetEnvironmentVariable("SHOPEE_RUN_SANDBOX"), "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool Missing(string name)
    {
        return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name));
    }
}
