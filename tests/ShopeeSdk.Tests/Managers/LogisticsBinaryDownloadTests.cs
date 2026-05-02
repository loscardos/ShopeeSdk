using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.Managers;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Generated.Logistics;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Managers;

public sealed class LogisticsBinaryDownloadTests
{
    [Fact]
    public async Task DownloadShippingDocumentFileAsync_ReturnsBinaryContent()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/logistics/download_shipping_document*")
            .Respond(_ => new HttpResponseMessage
            {
                Content = new ByteArrayContent([1, 2, 3])
                {
                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf") }
                }
            });

        var tokenStore = new InMemoryShopeeTokenStore();
        await tokenStore.StoreAsync(
            new ShopeeToken(600000, null, "access", "refresh", DateTimeOffset.UtcNow.AddHours(1), "token-req"),
            CancellationToken.None);

        var options = Options.Create(new ShopeeOptions { PartnerId = 123456, PartnerKey = "key", Region = ShopeeRegion.TestGlobal });
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = options.Value.ResolveBaseUrl();
        var client = new ShopeeClient(
            httpClient,
            options,
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);
        var tokenService = new ShopeeTokenService(tokenStore, new Mock<IAuthApi>().Object, options, new FixedShopeeClock(DateTimeOffset.UtcNow));
        var api = new LogisticsApi(client, tokenService);

        var file = await api.DownloadShippingDocumentFileAsync(
            600000,
            new DownloadShippingDocumentRequest("ORDER-1"),
            CancellationToken.None);

        file.Content.Should().Equal([1, 2, 3]);
        file.ContentType.Should().Be("application/pdf");
    }
}
