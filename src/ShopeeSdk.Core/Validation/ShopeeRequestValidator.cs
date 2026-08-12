using Loscardos.ShopeeSdk.Abstractions.Errors;

namespace Loscardos.ShopeeSdk.Core.Validation;

public static class ShopeeRequestValidator
{
    public static void EnsurePageSize(int pageSize, int min, int max, string fieldName)
    {
        if (pageSize < min || pageSize > max)
        {
            throw new ShopeeSdkException($"{fieldName} must be between {min} and {max}.");
        }
    }

    public static void EnsureTimeRange(long fromUnixSeconds, long toUnixSeconds, TimeSpan maxRange, string fieldName)
    {
        if (toUnixSeconds < fromUnixSeconds)
        {
            throw new ShopeeSdkException($"{fieldName} end must be greater than or equal to start.");
        }

        if (toUnixSeconds - fromUnixSeconds > (long)maxRange.TotalSeconds)
        {
            throw new ShopeeSdkException($"{fieldName} must not exceed {maxRange.TotalDays:0} days.");
        }
    }

    public static void EnsureRequiredString(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ShopeeSdkException($"{fieldName} is required.");
        }
    }

    public static void EnsureAny<T>(IReadOnlyCollection<T> values, string fieldName)
    {
        if (values.Count == 0)
        {
            throw new ShopeeSdkException($"{fieldName} must contain at least one value.");
        }
    }
}
