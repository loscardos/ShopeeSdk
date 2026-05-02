using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Auth;

public sealed class ShopeeTokenServiceConcurrencyTests
{
    [Fact]
    public async Task GetValidTokenAsync_RefreshesOnlyOnceForParallelExpiredTokenRequests()
    {
        var clock = new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000));
        var store = new InMemoryShopeeTokenStore();
        await store.StoreAsync(new ShopeeToken(600000, null, "old", "refresh", clock.UtcNow.AddMinutes(-1), "old-req"), CancellationToken.None);

        var authApi = new Mock<IAuthApi>();
        authApi
            .Setup(x => x.RefreshAsync(600000, null, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(25);
                var token = new ShopeeToken(600000, null, "new", "refresh-new", clock.UtcNow.AddHours(4), "new-req");
                await store.StoreAsync(token, CancellationToken.None);
                return token;
            });

        var service = new ShopeeTokenService(
            store,
            authApi.Object,
            Options.Create(new ShopeeOptions { TokenRefreshSkew = TimeSpan.FromMinutes(5) }),
            clock);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.GetValidTokenAsync(600000, null, CancellationToken.None));

        var tokens = await Task.WhenAll(tasks);

        tokens.Select(token => token.AccessToken).Distinct().Should().ContainSingle().Which.Should().Be("new");
        authApi.Verify(x => x.RefreshAsync(600000, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
