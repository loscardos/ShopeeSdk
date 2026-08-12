using Loscardos.ShopeeSdk.Abstractions.Http;

namespace Loscardos.ShopeeSdk.Abstractions.RateLimiting;

public interface IShopeeRateLimiter
{
    Task<bool> TryAcquireAsync(ShopeeRequest request, CancellationToken cancellationToken);
}
