using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Loscardos.ShopeeSdk.Abstractions.Managers;
using Loscardos.ShopeeSdk.Core.Auth;
using Loscardos.ShopeeSdk.Core.Http;
using Loscardos.ShopeeSdk.Core.Managers.Generated;
using Loscardos.ShopeeSdk.Core.RateLimiting;
using Loscardos.ShopeeSdk.Core.Time;
using Loscardos.ShopeeSdk.Tests.TestDoubles;
using GeneratedLogisticsApi = Loscardos.ShopeeSdk.Abstractions.Managers.Generated.ILogisticsApi;
using GeneratedOrderApi = Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IOrderApi;
using GeneratedPaymentApi = Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IPaymentApi;
using GeneratedProductApi = Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IProductApi;
using GeneratedShopApi = Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IShopApi;

namespace Loscardos.ShopeeSdk.IntegrationTests;

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

    public IOptions<Loscardos.ShopeeSdk.Abstractions.Configuration.ShopeeOptions> Options { get; }
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
