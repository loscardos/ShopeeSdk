using FluentAssertions;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeAuthSandboxTests
{
    [ShopeeSandboxFact]
    public void BuildAuthorizationUrl_UsesSandboxCredentials()
    {
        var fixture = new ShopeeSandboxFixture();

        var url = fixture.AuthApi.BuildAuthorizationUrl(ShopeeSandboxConfiguration.RedirectUri);

        url.Host.Should().Contain("sandbox.test-stable.shopee");
        url.Query.Should().Contain("partner_id=");
        url.Query.Should().Contain("timestamp=");
        url.Query.Should().Contain("redirect=");
        url.Query.Should().Contain("sign=");
    }

    [ShopeeSandboxAuthCodeFact]
    public async Task ExchangeCodeAsync_StoresSandboxShopToken()
    {
        var fixture = new ShopeeSandboxFixture();

        var token = await fixture.AuthApi.ExchangeCodeAsync(
            ShopeeSandboxConfiguration.AuthCode,
            ShopeeSandboxConfiguration.ShopId,
            null,
            CancellationToken.None);

        token.ShopId.Should().Be(ShopeeSandboxConfiguration.ShopId);
        token.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.RefreshToken.Should().NotBeNullOrWhiteSpace();
        token.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [ShopeeSandboxRefreshTokenFact]
    public async Task RefreshAsync_RotatesSandboxShopToken()
    {
        var fixture = new ShopeeSandboxFixture();
        await fixture.StoreConfiguredShopTokenAsync();

        var token = await fixture.AuthApi.RefreshAsync(ShopeeSandboxConfiguration.ShopId, null, CancellationToken.None);

        token.ShopId.Should().Be(ShopeeSandboxConfiguration.ShopId);
        token.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.RefreshToken.Should().NotBeNullOrWhiteSpace();
        token.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }
}
