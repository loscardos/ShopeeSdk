using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Generated.Order;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Http;

public sealed class ShopeeClientLoggingTests
{
    [Fact]
    public async Task SendAsync_LogsPathAndShopeeRequestId()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/shop/get_shop_info*")
            .Respond("application/json", """
            { "request_id": "req-log", "error": "", "message": "", "response": { } }
            """);

        var logger = new ListLogger<ShopeeClient>();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://openplatform.sandbox.test-stable.shopee.sg/api/v2/");

        var client = new ShopeeClient(
            httpClient,
            Options.Create(new ShopeeOptions { PartnerId = 123456, PartnerKey = "key", Region = ShopeeRegion.TestGlobal }),
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)),
            new NoopShopeeRateLimiter(),
            logger);

        await client.SendAsync<object>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/shop/get_shop_info", new Dictionary<string, object?>(), null, null),
            CancellationToken.None);

        logger.Messages.Should().Contain(message => message.Contains("/shop/get_shop_info", StringComparison.Ordinal));
        logger.Messages.Should().Contain(message => message.Contains("req-log", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SendAsync_LogsSafeJsonContractDiagnosticsWithoutPayload()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/order/get_order_list*")
            .Respond("application/json", """
            {
              "request_id": "req-contract",
              "error": "",
              "message": "",
              "response": { "more": "sensitive-payload-marker" }
            }
            """);
        var logger = new ListLogger<ShopeeClient>();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://openplatform.sandbox.test-stable.shopee.sg/api/v2/");
        var client = new ShopeeClient(
            httpClient,
            Options.Create(new ShopeeOptions
            {
                PartnerId = 123456,
                PartnerKey = "key",
                Region = ShopeeRegion.TestGlobal
            }),
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)),
            new NoopShopeeRateLimiter(),
            logger);

        var act = async () => await client.SendAsync<OrderGetOrderListResponse>(
            new ShopeeRequest(
                ShopeeRequestMethod.Get,
                "/order/get_order_list",
                new Dictionary<string, object?>(),
                null,
                null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ShopeeSdk.Abstractions.Errors.ShopeeSdkException>();
        logger.Messages.Should().Contain(message =>
            message.Contains("JsonPath=$.more", StringComparison.Ordinal)
            && message.Contains("ResponseType=OrderGetOrderListResponse", StringComparison.Ordinal)
            && message.Contains("ContentType=application/json", StringComparison.Ordinal)
            && message.Contains("BodyLength=", StringComparison.Ordinal));
        logger.Messages.Should().NotContain(message =>
            message.Contains("sensitive-payload-marker", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SendAsync_LogsSafeEnvelopeKindsForNonStringError()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/order/get_order_list*")
            .Respond("application/json", """
            {
              "request_id": "req-envelope",
              "error": { "detail": "sensitive-envelope-marker" },
              "message": "",
              "response": { }
            }
            """);
        var logger = new ListLogger<ShopeeClient>();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://openplatform.sandbox.test-stable.shopee.sg/api/v2/");
        var client = new ShopeeClient(
            httpClient,
            Options.Create(new ShopeeOptions
            {
                PartnerId = 123456,
                PartnerKey = "key",
                Region = ShopeeRegion.TestGlobal
            }),
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)),
            new NoopShopeeRateLimiter(),
            logger);

        var act = async () => await client.SendAsync<OrderGetOrderListResponse>(
            new ShopeeRequest(
                ShopeeRequestMethod.Get,
                "/order/get_order_list",
                new Dictionary<string, object?>(),
                null,
                null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ShopeeSdk.Abstractions.Errors.ShopeeSdkException>();
        logger.Messages.Should().Contain(message =>
            message.Contains("ExceptionType=InvalidOperationException", StringComparison.Ordinal)
            && message.Contains("RootKind=Object", StringComparison.Ordinal)
            && message.Contains("ResponseKind=Object", StringComparison.Ordinal)
            && message.Contains("ErrorKind=Object", StringComparison.Ordinal)
            && message.Contains("MessageKind=String", StringComparison.Ordinal));
        logger.Messages.Should().NotContain(message =>
            message.Contains("sensitive-envelope-marker", StringComparison.Ordinal));
    }
}
