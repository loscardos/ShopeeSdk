using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace ShopeeSdk.Tests.Auth;

public sealed class ShopeeAuthApiTests
{
    [Fact]
    public void BuildAuthorizationUrl_ContainsPartnerTimestampRedirectAndSign()
    {
        var api = CreateApi(new MockHttpMessageHandler());

        var url = api.BuildAuthorizationUrl(new Uri("https://omni.example.test/shopee/callback"));

        url.ToString().Should().StartWith("https://partner.test-stable.shopeemobile.com/api/v2/shop/auth_partner?");
        url.Query.Should().Contain("partner_id=123456");
        url.Query.Should().Contain("timestamp=1610000000");
        url.Query.Should().Contain("redirect=https%3A%2F%2Fomni.example.test%2Fshopee%2Fcallback");
        url.Query.Should().Contain("sign=");
    }

    [Fact]
    public async Task ExchangeCodeAsync_StoresToken()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Post, "https://partner.test-stable.shopeemobile.com/api/v2/auth/token/get*")
            .Respond("application/json", """
            {
              "request_id": "auth-1",
              "error": "",
              "message": "",
              "access_token": "access-1",
              "refresh_token": "refresh-1",
              "expire_in": 14400
            }
            """);

        var store = new InMemoryShopeeTokenStore();
        var api = CreateApi(mockHttp, store);

        var token = await api.ExchangeCodeAsync("code-1", 600000, null, CancellationToken.None);

        token.AccessToken.Should().Be("access-1");
        var stored = await store.GetAsync(600000, null, CancellationToken.None);
        stored.Should().NotBeNull();
    }

    private static ShopeeAuthApi CreateApi(MockHttpMessageHandler mockHttp, InMemoryShopeeTokenStore? store = null)
    {
        var options = Options.Create(new ShopeeOptions
        {
            PartnerId = 123456,
            PartnerKey = "test_partner_key",
            Region = ShopeeRegion.TestGlobal
        });

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = options.Value.ResolveBaseUrl();
        var client = new ShopeeClient(
            httpClient,
            options,
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1610000000)),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);

        return new ShopeeAuthApi(client, options, new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1610000000)), store ?? new InMemoryShopeeTokenStore());
    }
}
