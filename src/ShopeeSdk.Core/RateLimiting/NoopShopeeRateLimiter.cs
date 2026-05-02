using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.RateLimiting;

namespace ShopeeSdk.Core.RateLimiting;

public sealed class NoopShopeeRateLimiter : IShopeeRateLimiter
{
    public Task<bool> TryAcquireAsync(ShopeeRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
