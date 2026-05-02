using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.Managers;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Core.Time;
using ShopeeSdk.Generated.Order;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeReadOnlySandboxTests
{
    [ShopeeSandboxTokenFact]
    public async Task GetOrderListAsync_CallsSandboxReadOnlyEndpoint()
    {
        var options = Options.Create(ShopeeSandboxConfiguration.CreateOptions());
        var tokenStore = new InMemoryShopeeTokenStore();
        await tokenStore.StoreAsync(ShopeeSandboxConfiguration.CreateToken(), CancellationToken.None);

        var client = new ShopeeClient(
            new HttpClient { BaseAddress = options.Value.ResolveBaseUrl() },
            options,
            new SystemShopeeClock(),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);
        var tokenService = new ShopeeTokenService(tokenStore, Moq.Mock.Of<IAuthApi>(), options, new SystemShopeeClock());
        var orderApi = new OrderApi(client, tokenService);

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var response = await orderApi.GetOrderListAsync(
            ShopeeSandboxConfiguration.ShopId,
            new GetOrderListRequest("update_time", now - 3600, now, 20),
            CancellationToken.None);

        response.OrderList.Should().NotBeNull();
    }
}
