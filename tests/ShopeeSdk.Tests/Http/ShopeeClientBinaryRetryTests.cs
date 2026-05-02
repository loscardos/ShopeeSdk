using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Http;

public sealed class ShopeeClientBinaryRetryTests
{
    [Fact]
    public async Task SendForBytesAsync_RetriesTransientHttpFailure()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .Expect(HttpMethod.Get, "https://partner.test-stable.shopeemobile.com/api/v2/logistics/download_shipping_document*")
            .Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp
            .Expect(HttpMethod.Get, "https://partner.test-stable.shopeemobile.com/api/v2/logistics/download_shipping_document*")
            .Respond(_ => new HttpResponseMessage
            {
                Content = new ByteArrayContent([1, 2, 3])
                {
                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf") }
                }
            });

        var client = CreateClient(mockHttp, maxRetries: 1);

        var response = await client.SendForBytesAsync(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/download_shipping_document", new Dictionary<string, object?>(), null, null),
            CancellationToken.None);

        response.Content.Should().Equal([1, 2, 3]);
        response.ContentType.Should().Be("application/pdf");
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task SendForBytesAsync_ThrowsShopeeApiExceptionForRateLimitStatus()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .Expect(HttpMethod.Get, "https://partner.test-stable.shopeemobile.com/api/v2/logistics/download_shipping_document*")
            .Respond(System.Net.HttpStatusCode.TooManyRequests);

        var client = CreateClient(mockHttp, maxRetries: 0);

        var act = async () => await client.SendForBytesAsync(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/download_shipping_document", new Dictionary<string, object?>(), null, null),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ShopeeSdk.Abstractions.Errors.ShopeeApiException>();
        exception.Which.Category.Should().Be(ShopeeErrorCategory.RateLimit);
        exception.Which.Path.Should().Be("/logistics/download_shipping_document");
    }

    private static ShopeeClient CreateClient(MockHttpMessageHandler mockHttp, int maxRetries)
    {
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://partner.test-stable.shopeemobile.com/api/v2/");

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
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);
    }
}
