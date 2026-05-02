using Microsoft.Extensions.DependencyInjection;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Abstractions.Webhooks;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.Managers;
using ShopeeSdk.Core.Time;
using ShopeeSdk.Core.Webhooks;

namespace ShopeeSdk.Extensions.DependencyInjection;

public static class ShopeeServiceCollectionExtensions
{
    public static IServiceCollection AddShopeeSdk(
        this IServiceCollection services,
        Action<ShopeeOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IShopeeClock, SystemShopeeClock>();
        services.AddScoped<ShopeeAuthApi>();
        services.AddScoped<IAuthApi>(provider => provider.GetRequiredService<ShopeeAuthApi>());
        services.AddScoped<ShopeeTokenService>();
        services.AddSingleton<IShopeeWebhookVerifier, ShopeeWebhookVerifier>();
        services.AddScoped<IProductApi, ProductApi>();
        services.AddScoped<IOrderApi, OrderApi>();
        services.AddScoped<ILogisticsApi, LogisticsApi>();
        services.AddScoped<IPaymentApi, PaymentApi>();
        services.AddScoped<IReturnsApi, ReturnsApi>();
        services.AddHttpClient<IShopeeClient, ShopeeClient>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ShopeeOptions>>().Value;
            httpClient.BaseAddress = options.ResolveBaseUrl();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("InternalShopeeSdk/0.1");
        });

        return services;
    }
}
