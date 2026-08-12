using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Abstractions.Errors;
using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Core.Http;
using Loscardos.ShopeeSdk.Core.RateLimiting;
using Loscardos.ShopeeSdk.Tests.TestDoubles;

namespace Loscardos.ShopeeSdk.Tests.Http;

public sealed class ShopeeClientObservabilityTests
{
    [Fact]
    public async Task SendAsync_LogsShopIdStatusRequestIdErrorAndCategory()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://openplatform.sandbox.test-stable.shopee.sg/api/v2/order/get_order_list*")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", """
            { "request_id": "req-observe", "error": "error_param", "message": "bad param" }
            """);

        var logger = new ListLogger<ShopeeClient>();
        var client = CreateClient(mockHttp, logger);

        var act = async () => await client.SendAsync<object>(
            new ShopeeRequest(
                ShopeeRequestMethod.Get,
                "/order/get_order_list",
                new Dictionary<string, object?> { ["access_token"] = "access" },
                null,
                new ShopeeRequestAuth(600000, null)),
            CancellationToken.None);

        await act.Should().ThrowAsync<ShopeeApiException>();
        logger.Levels.Should().Contain(LogLevel.Information);
        logger.Messages.Should().Contain(message => message.Contains("ShopId=600000", StringComparison.Ordinal));
        logger.Messages.Should().Contain(message => message.Contains("StatusCode=200", StringComparison.Ordinal));
        logger.Messages.Should().Contain(message => message.Contains("RequestId=req-observe", StringComparison.Ordinal));
        logger.Messages.Should().Contain(message => message.Contains("Error=error_param", StringComparison.Ordinal));
        logger.Messages.Should().Contain(message => message.Contains("Category=Validation", StringComparison.Ordinal));
    }

    private static ShopeeClient CreateClient(MockHttpMessageHandler mockHttp, ListLogger<ShopeeClient> logger)
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
                MaxTransientRetries = 0
            }),
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)),
            new NoopShopeeRateLimiter(),
            logger);
    }
}
