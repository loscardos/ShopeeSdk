using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Managers.Generated;
using ShopeeSdk.Generated.Common;
using ShopeeSdk.Generated.Logistics;
using ShopeeSdk.Generated.Media;
using ShopeeSdk.Generated.Merchant;
using ShopeeSdk.Generated.Product;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Managers;

public sealed class GeneratedManagerRequestSemanticsTests
{
    [Fact]
    public async Task ShopQueryEndpoint_SendsPathMethodAuthTokenAndQuery()
    {
        var client = new Mock<IShopeeClient>();
        client
            .Setup(x => x.SendAsync<ProductGetCategoryResponse>(It.IsAny<ShopeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShopeeEnvelope<ProductGetCategoryResponse>("req-1", "", "", new ProductGetCategoryResponse()));

        var api = new ProductApi(client.Object, await CreateTokenServiceAsync(shopId: 600000, merchantId: null));

        await api.GetCategoryAsync(600000, new ProductGetCategoryRequest(Language: "en"), CancellationToken.None);

        client.Verify(x => x.SendAsync<ProductGetCategoryResponse>(
            It.Is<ShopeeRequest>(request =>
                request.Method == ShopeeRequestMethod.Get
                && request.Path == "/product/get_category"
                && request.Body == null
                && request.Auth!.ShopId == 600000
                && request.Auth.MerchantId == null
                && request.Query["access_token"]!.ToString() == "access-shop"
                && request.Query["language"]!.ToString() == "en"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShopJsonEndpoint_SendsBodyAndAuthToken()
    {
        var client = new Mock<IShopeeClient>();
        client
            .Setup(x => x.SendAsync<LogisticsShipOrderResponse>(It.IsAny<ShopeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShopeeEnvelope<LogisticsShipOrderResponse>("req-2", "", "", new LogisticsShipOrderResponse()));

        var api = new LogisticsApi(client.Object, await CreateTokenServiceAsync(shopId: 600000, merchantId: null));
        var requestBody = new LogisticsShipOrderRequest(OrderSn: "250503ABC");

        await api.ShipOrderAsync(600000, requestBody, CancellationToken.None);

        client.Verify(x => x.SendAsync<LogisticsShipOrderResponse>(
            It.Is<ShopeeRequest>(request =>
                request.Method == ShopeeRequestMethod.Post
                && request.Path == "/logistics/ship_order"
                && ReferenceEquals(request.Body, requestBody)
                && request.ContentKind == ShopeeRequestContentKind.Json
                && request.Auth!.ShopId == 600000
                && request.Query["access_token"]!.ToString() == "access-shop"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MerchantQueryEndpoint_SendsMerchantAuthAndQuery()
    {
        var client = new Mock<IShopeeClient>();
        client
            .Setup(x => x.SendAsync<MerchantGetShopListByMerchantResponse>(It.IsAny<ShopeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShopeeEnvelope<MerchantGetShopListByMerchantResponse>("req-3", "", "", new MerchantGetShopListByMerchantResponse()));

        var api = new MerchantApi(client.Object, await CreateTokenServiceAsync(shopId: null, merchantId: 700000));

        await api.GetShopListByMerchantAsync(700000, new MerchantGetShopListByMerchantRequest(PageNo: 1, PageSize: 50), CancellationToken.None);

        client.Verify(x => x.SendAsync<MerchantGetShopListByMerchantResponse>(
            It.Is<ShopeeRequest>(request =>
                request.Method == ShopeeRequestMethod.Get
                && request.Path == "/merchant/get_shop_list_by_merchant"
                && request.Body == null
                && request.Auth!.ShopId == null
                && request.Auth.MerchantId == 700000
                && request.Query["access_token"]!.ToString() == "access-merchant"
                && request.Query["page_no"]!.ToString() == "1"
                && request.Query["page_size"]!.ToString() == "50"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublicMultipartEndpoint_SendsMultipartBodyWithoutAuth()
    {
        var client = new Mock<IShopeeClient>();
        client
            .Setup(x => x.SendAsync<MediaUploadImageResponse>(It.IsAny<ShopeeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShopeeEnvelope<MediaUploadImageResponse>("req-4", "", "", new MediaUploadImageResponse()));

        var api = new MediaApi(client.Object);
        var requestBody = new MediaUploadImageRequest(
            Business: 1,
            Scene: 1,
            Images: new ShopeeUploadFile("image.jpg", "image/jpeg", [1, 2, 3]));

        await api.UploadImageAsync(requestBody, CancellationToken.None);

        client.Verify(x => x.SendAsync<MediaUploadImageResponse>(
            It.Is<ShopeeRequest>(request =>
                request.Method == ShopeeRequestMethod.Post
                && request.Path == "/media/upload_image"
                && ReferenceEquals(request.Body, requestBody)
                && request.Auth == null
                && request.Query.Count == 0
                && request.ContentKind == ShopeeRequestContentKind.Multipart),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static async Task<ShopeeTokenService> CreateTokenServiceAsync(long? shopId, long? merchantId)
    {
        var tokenStore = new InMemoryShopeeTokenStore();
        await tokenStore.StoreAsync(
            new ShopeeToken(
                shopId,
                merchantId,
                merchantId is null ? "access-shop" : "access-merchant",
                "refresh",
                DateTimeOffset.UtcNow.AddHours(1),
                "request-token"),
            CancellationToken.None);

        var authApi = new Mock<IAuthApi>();
        return new ShopeeTokenService(
            tokenStore,
            authApi.Object,
            Options.Create(new ShopeeOptions()),
            new FixedShopeeClock(DateTimeOffset.UtcNow));
    }
}
