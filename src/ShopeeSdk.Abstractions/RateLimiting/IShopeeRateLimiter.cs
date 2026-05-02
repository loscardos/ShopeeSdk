using ShopeeSdk.Abstractions.Http;

namespace ShopeeSdk.Abstractions.RateLimiting;

public interface IShopeeRateLimiter
{
    Task<bool> TryAcquireAsync(ShopeeRequest request, CancellationToken cancellationToken);
}
