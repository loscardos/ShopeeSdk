using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.RateLimiting;

namespace ShopeeSdk.Core.RateLimiting;

public sealed class PartitionedShopeeRateLimiter(IOptions<ShopeeOptions> options) : IShopeeRateLimiter
{
    private readonly ConcurrentDictionary<string, FixedWindowRateLimiter> _limiters = new();
    private readonly ShopeeOptions _options = options.Value;

    public Task<bool> TryAcquireAsync(ShopeeRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = $"{request.Auth?.ShopId?.ToString() ?? "public"}:{request.Path}";
        var limiter = _limiters.GetOrAdd(key, _ => new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = _options.RateLimitPermitLimit,
            QueueLimit = _options.RateLimitQueueLimit,
            Window = _options.RateLimitWindow,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        }));

        using var lease = limiter.AttemptAcquire(1);
        return Task.FromResult(lease.IsAcquired);
    }
}
