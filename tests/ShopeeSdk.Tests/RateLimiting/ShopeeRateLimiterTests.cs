using FluentAssertions;
using Microsoft.Extensions.Options;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Core.RateLimiting;

namespace Loscardos.ShopeeSdk.Tests.RateLimiting;

public sealed class ShopeeRateLimiterTests
{
    [Fact]
    public async Task PartitionedLimiter_AllowsConfiguredPermitCount()
    {
        var limiter = new PartitionedShopeeRateLimiter(Options.Create(new ShopeeOptions
        {
            RateLimitPermitLimit = 1,
            RateLimitQueueLimit = 0,
            RateLimitWindow = TimeSpan.FromMinutes(1)
        }));

        var first = await limiter.TryAcquireAsync(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/order/get_order_list", new Dictionary<string, object?>(), null, new ShopeeRequestAuth(600000, null)),
            CancellationToken.None);
        var second = await limiter.TryAcquireAsync(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/order/get_order_list", new Dictionary<string, object?>(), null, new ShopeeRequestAuth(600000, null)),
            CancellationToken.None);

        first.Should().BeTrue();
        second.Should().BeFalse();
    }
}
