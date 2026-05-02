using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Managers;
using ShopeeSdk.Generated.Product;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Managers;

public sealed class ProductApiTests
{
    [Fact]
    public async Task GetItemListAsync_SendsExpectedPathAndAuth()
    {
        var client = new Mock<IShopeeClient>();
        client
            .Setup(x => x.SendAsync<GetItemListResponse>(It.IsAny<ShopeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShopeeEnvelope<GetItemListResponse>("req-1", "", "", new GetItemListResponse([], 0, false, null)));

        var tokenStore = new InMemoryShopeeTokenStore();
        await tokenStore.StoreAsync(
            new ShopeeToken(600000, null, "access-1", "refresh-1", DateTimeOffset.UtcNow.AddHours(1), "req-token"),
            CancellationToken.None);

        var authApi = new Mock<IAuthApi>();
        var tokenService = new ShopeeTokenService(
            tokenStore,
            authApi.Object,
            Options.Create(new ShopeeOptions()),
            new FixedShopeeClock(DateTimeOffset.UtcNow));

        var api = new ProductApi(client.Object, tokenService);
        var result = await api.GetItemListAsync(600000, new GetItemListRequest(0, 10), CancellationToken.None);

        result.TotalCount.Should().Be(0);
        client.Verify(x => x.SendAsync<GetItemListResponse>(
            It.Is<ShopeeRequest>(request =>
                request.Path == "/product/get_item_list"
                && request.Auth!.ShopId == 600000
                && request.Query["access_token"]!.ToString() == "access-1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
