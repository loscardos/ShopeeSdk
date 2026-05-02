using ShopeeSdk.Abstractions.Http;

namespace ShopeeSdk.Core.Http;

public static class ShopeeErrorClassifier
{
    public static ShopeeErrorCategory Classify(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return ShopeeErrorCategory.Unknown;
        }

        var normalized = error.Trim().ToLowerInvariant();

        if (normalized.Contains("access_token", StringComparison.Ordinal)
            || normalized.Contains("acceess_token", StringComparison.Ordinal)
            || normalized is "error_auth")
        {
            return ShopeeErrorCategory.Authentication;
        }

        if (normalized.Contains("sign", StringComparison.Ordinal))
        {
            return ShopeeErrorCategory.Signature;
        }

        if (normalized.Contains("permission", StringComparison.Ordinal))
        {
            return ShopeeErrorCategory.Permission;
        }

        if (normalized.Contains("rate", StringComparison.Ordinal) || normalized.Contains("limit", StringComparison.Ordinal))
        {
            return ShopeeErrorCategory.RateLimit;
        }

        if (normalized.Contains("busy", StringComparison.Ordinal)
            || normalized.Contains("server", StringComparison.Ordinal)
            || normalized.Contains("network", StringComparison.Ordinal)
            || normalized.Contains("inner", StringComparison.Ordinal))
        {
            return ShopeeErrorCategory.Transient;
        }

        if (normalized.Contains("param", StringComparison.Ordinal) || normalized.Contains("validation", StringComparison.Ordinal))
        {
            return ShopeeErrorCategory.Validation;
        }

        return ShopeeErrorCategory.Unknown;
    }
}
