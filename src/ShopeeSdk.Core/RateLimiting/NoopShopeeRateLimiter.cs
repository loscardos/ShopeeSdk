using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Abstractions.RateLimiting;

namespace Loscardos.ShopeeSdk.Core.RateLimiting;

public sealed class NoopShopeeRateLimiter : IShopeeRateLimiter
{
    public Task<bool> TryAcquireAsync(ShopeeRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
