using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using System.Text.Json;
using Loscardos.ShopeeSdk.Abstractions.Auth;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Abstractions.Managers;
using Loscardos.ShopeeSdk.Core.Auth;
using Loscardos.ShopeeSdk.Core.Http;
using Loscardos.ShopeeSdk.Core.Managers;
using Loscardos.ShopeeSdk.Core.RateLimiting;
using Loscardos.ShopeeSdk.Generated.Logistics;
using Loscardos.ShopeeSdk.Tests.TestDoubles;

namespace Loscardos.ShopeeSdk.Tests.Managers;

public sealed class LogisticsBinaryDownloadTests
{
    [Fact]
    public async Task DownloadShippingDocumentFileAsync_ReturnsBinaryContent()
    {
        var mockHttp = new MockHttpMessageHandler();
        string? requestBody = null;
        mockHttp
            .When(HttpMethod.Post, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/logistics/download_shipping_document*")
            .Respond(request =>
            {
                requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return new HttpResponseMessage
                {
                    Content = new ByteArrayContent([1, 2, 3])
                    {
                        Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf") }
                    }
                };
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
            new DownloadShippingDocumentRequest("ORDER-1", "PACKAGE-1", "NORMAL_AIR_WAYBILL"),
            CancellationToken.None);

        file.Content.Should().Equal([1, 2, 3]);
        file.ContentType.Should().Be("application/pdf");
        using var json = JsonDocument.Parse(requestBody!);
        json.RootElement.GetProperty("shipping_document_type").GetString()
            .Should().Be("NORMAL_AIR_WAYBILL");
        var order = json.RootElement.GetProperty("order_list")[0];
        order.GetProperty("order_sn").GetString().Should().Be("ORDER-1");
        order.GetProperty("package_number").GetString().Should().Be("PACKAGE-1");
    }
}
