using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Abstractions.Errors;
using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Core.Http;
using Loscardos.ShopeeSdk.Core.RateLimiting;
using Loscardos.ShopeeSdk.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Loscardos.ShopeeSdk.Tests.Http;

public sealed class ShopeeClientTests
{
    [Fact]
    public async Task SendAsync_AddsCommonParamsAndReturnsEnvelope()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/product/get_item_list*")
            .WithQueryString("partner_id", "123456")
            .WithQueryString("timestamp", "1610000000")
            .WithQueryString("offset", "0")
            .WithQueryString("page_size", "10")
            .Respond("application/json", """
            {
              "request_id": "req-1",
              "error": "",
              "message": "",
              "response": { "total_count": 0, "item": [], "has_next_page": false }
            }
            """);

        var client = CreateClient(mockHttp);

        var response = await client.SendAsync<object>(
            new ShopeeRequest(
                ShopeeRequestMethod.Get,
                "/product/get_item_list",
                new Dictionary<string, object?> { ["offset"] = 0, ["page_size"] = 10 },
                null,
                null),
            CancellationToken.None);

        response.RequestId.Should().Be("req-1");
        response.Error.Should().BeEmpty();
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task SendAsync_ThrowsTypedApiExceptionForShopeeError()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/product/get_item_list*")
            .Respond(HttpStatusCode.OK, "application/json", """
            {
              "request_id": "req-2",
              "error": "error_sign",
              "message": "Wrong sign."
            }
            """);

        var client = CreateClient(mockHttp);

        var act = async () => await client.SendAsync<object>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/product/get_item_list", new Dictionary<string, object?>(), null, null),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ShopeeApiException>();
        exception.Which.Error.Should().Be("error_sign");
        exception.Which.RequestId.Should().Be("req-2");
        exception.Which.Path.Should().Be("/product/get_item_list");
    }

    [Fact]
    public async Task SendAsync_RetriesTransientHttpFailure()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .Expect(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/product/get_item_list*")
            .Respond(HttpStatusCode.InternalServerError, "application/json", """
            { "request_id": "req-fail", "error": "error_server", "message": "try later" }
            """);
        mockHttp
            .Expect(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/product/get_item_list*")
            .Respond("application/json", """
            { "request_id": "req-ok", "error": "", "message": "", "response": { "total_count": 0 } }
            """);

        var client = CreateClient(mockHttp, maxRetries: 1);

        var response = await client.SendAsync<object>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/product/get_item_list", new Dictionary<string, object?>(), null, null),
            CancellationToken.None);

        response.RequestId.Should().Be("req-ok");
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task SendAsync_WithMerchantAuth_AddsMerchantIdToQuery()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/merchant/get_shop_list_by_merchant*")
            .WithQueryString("access_token", "merchant-token")
            .WithQueryString("merchant_id", "900001")
            .Respond("application/json", """
            {
              "request_id": "req-merchant",
              "error": "",
              "message": "",
              "response": {}
            }
            """);

        var client = CreateClient(mockHttp);

        var response = await client.SendAsync<object>(
            new ShopeeRequest(
                ShopeeRequestMethod.Get,
                "/merchant/get_shop_list_by_merchant",
                new Dictionary<string, object?> { ["access_token"] = "merchant-token" },
                null,
                new ShopeeRequestAuth(null, 900001)),
            CancellationToken.None);

        response.RequestId.Should().Be("req-merchant");
        mockHttp.VerifyNoOutstandingExpectation();
    }

    private static ShopeeClient CreateClient(MockHttpMessageHandler mockHttp, int maxRetries = 2)
    {
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://openplatform.sandbox.test-stable.shopee.sg/api/v2/");
        return new ShopeeClient(
            httpClient,
            Options.Create(new ShopeeOptions
            {
                PartnerId = 123456,
                PartnerKey = "test_partner_key",
                Region = ShopeeRegion.TestGlobal,
                MaxTransientRetries = maxRetries,
                RetryBaseDelay = TimeSpan.Zero
            }),
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1610000000)),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);
    }
}
