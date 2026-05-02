using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeAuthSandboxTests
{
    [ShopeeSandboxFact]
    public void BuildAuthorizationUrl_UsesSandboxCredentials()
    {
        var options = Options.Create(ShopeeSandboxConfiguration.CreateOptions());
        var client = new ShopeeClient(
            new HttpClient { BaseAddress = options.Value.ResolveBaseUrl() },
            options,
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);
        var auth = new ShopeeAuthApi(client, options, new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000)), new InMemoryShopeeTokenStore());

        var url = auth.BuildAuthorizationUrl(new Uri("https://localhost/shopee/callback"));

        url.ToString().Should().Contain("partner_id=");
        url.ToString().Should().Contain("sign=");
        url.ToString().Should().Contain("openplatform.sandbox.test-stable.shopee.sg");
    }
}
