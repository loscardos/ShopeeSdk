using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.Managers.Generated;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Core.Time;
using ShopeeSdk.Tests.TestDoubles;
using GeneratedLogisticsApi = ShopeeSdk.Abstractions.Managers.Generated.ILogisticsApi;
using GeneratedOrderApi = ShopeeSdk.Abstractions.Managers.Generated.IOrderApi;
using GeneratedPaymentApi = ShopeeSdk.Abstractions.Managers.Generated.IPaymentApi;
using GeneratedProductApi = ShopeeSdk.Abstractions.Managers.Generated.IProductApi;
using GeneratedShopApi = ShopeeSdk.Abstractions.Managers.Generated.IShopApi;

namespace ShopeeSdk.IntegrationTests;

public sealed class ShopeeSandboxFixture
{
    public ShopeeSandboxFixture()
    {
        Options = Microsoft.Extensions.Options.Options.Create(ShopeeSandboxConfiguration.CreateOptions());
        TokenStore = new InMemoryShopeeTokenStore();
        Client = new ShopeeClient(
            new HttpClient { BaseAddress = Options.Value.ResolveBaseUrl() },
            Options,
            new SystemShopeeClock(),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);
        AuthApi = new ShopeeAuthApi(Client, Options, new SystemShopeeClock(), TokenStore);
        TokenService = new ShopeeTokenService(TokenStore, Mock.Of<IAuthApi>(), Options, new SystemShopeeClock());
    }

    public IOptions<ShopeeSdk.Abstractions.Configuration.ShopeeOptions> Options { get; }
    public InMemoryShopeeTokenStore TokenStore { get; }
    public ShopeeClient Client { get; }
    public ShopeeAuthApi AuthApi { get; }
    public ShopeeTokenService TokenService { get; }

    public async Task StoreConfiguredShopTokenAsync()
    {
        await TokenStore.StoreAsync(ShopeeSandboxConfiguration.CreateToken(), CancellationToken.None);
    }

    public GeneratedProductApi CreateProductApi() => new ProductApi(Client, TokenService);
    public GeneratedOrderApi CreateOrderApi() => new OrderApi(Client, TokenService);
    public GeneratedShopApi CreateShopApi() => new ShopApi(Client, TokenService);
    public GeneratedLogisticsApi CreateLogisticsApi() => new LogisticsApi(Client, TokenService);
    public GeneratedPaymentApi CreatePaymentApi() => new PaymentApi(Client, TokenService);
}
