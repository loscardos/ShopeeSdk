using Microsoft.Extensions.DependencyInjection;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Abstractions.Managers;
using Loscardos.ShopeeSdk.Abstractions.RateLimiting;
using Loscardos.ShopeeSdk.Abstractions.Webhooks;
using Loscardos.ShopeeSdk.Core.Auth;
using Loscardos.ShopeeSdk.Core.Http;
using Loscardos.ShopeeSdk.Core.Managers;
using Loscardos.ShopeeSdk.Core.RateLimiting;
using Loscardos.ShopeeSdk.Core.Time;
using Loscardos.ShopeeSdk.Core.Webhooks;

namespace Loscardos.ShopeeSdk.Extensions.DependencyInjection;

public static class ShopeeServiceCollectionExtensions
{
    public static IServiceCollection AddShopeeSdk(
        this IServiceCollection services,
        Action<ShopeeOptions> configure)
        => services.AddShopeeSdk(configure, _ => { });

    public static IServiceCollection AddShopeeSdk(
        this IServiceCollection services,
        Action<ShopeeOptions> configure,
        Action<ShopeeWebhookOptions> configureWebhooks)
    {
        ArgumentNullException.ThrowIfNull(configureWebhooks);

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
        services.AddSingleton<IShopeeWebhookIdempotencyKeyFactory, ShopeeWebhookIdempotencyKeyFactory>();
        services.AddSingleton(_ =>
        {
            var options = new ShopeeWebhookOptions();
            configureWebhooks(options);
            return options;
        });
        services.AddSingleton<ShopeeWebhookTimestampValidator>();
        services.AddSingleton<IShopeeWebhookParser, ShopeeWebhookParser>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IAccountHealthApi, Loscardos.ShopeeSdk.Core.Managers.Generated.AccountHealthApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IAddOnDealApi, Loscardos.ShopeeSdk.Core.Managers.Generated.AddOnDealApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IAdsApi, Loscardos.ShopeeSdk.Core.Managers.Generated.AdsApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IAmsApi, Loscardos.ShopeeSdk.Core.Managers.Generated.AmsApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IBundleDealApi, Loscardos.ShopeeSdk.Core.Managers.Generated.BundleDealApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IDiscountApi, Loscardos.ShopeeSdk.Core.Managers.Generated.DiscountApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IFbsApi, Loscardos.ShopeeSdk.Core.Managers.Generated.FbsApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IFirstMileApi, Loscardos.ShopeeSdk.Core.Managers.Generated.FirstMileApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IFollowPrizeApi, Loscardos.ShopeeSdk.Core.Managers.Generated.FollowPrizeApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IGlobalProductApi, Loscardos.ShopeeSdk.Core.Managers.Generated.GlobalProductApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.ILivestreamApi, Loscardos.ShopeeSdk.Core.Managers.Generated.LivestreamApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.ILogisticsApi, Loscardos.ShopeeSdk.Core.Managers.Generated.LogisticsApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IMediaApi, Loscardos.ShopeeSdk.Core.Managers.Generated.MediaApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IMediaSpaceApi, Loscardos.ShopeeSdk.Core.Managers.Generated.MediaSpaceApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IMerchantApi, Loscardos.ShopeeSdk.Core.Managers.Generated.MerchantApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IOrderApi, Loscardos.ShopeeSdk.Core.Managers.Generated.OrderApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IPaymentApi, Loscardos.ShopeeSdk.Core.Managers.Generated.PaymentApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IProductApi, Loscardos.ShopeeSdk.Core.Managers.Generated.ProductApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IPublicApi, Loscardos.ShopeeSdk.Core.Managers.Generated.PublicApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IPushApi, Loscardos.ShopeeSdk.Core.Managers.Generated.PushApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IReturnsApi, Loscardos.ShopeeSdk.Core.Managers.Generated.ReturnsApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.ISbsApi, Loscardos.ShopeeSdk.Core.Managers.Generated.SbsApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IShopApi, Loscardos.ShopeeSdk.Core.Managers.Generated.ShopApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IShopCategoryApi, Loscardos.ShopeeSdk.Core.Managers.Generated.ShopCategoryApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IShopFlashSaleApi, Loscardos.ShopeeSdk.Core.Managers.Generated.ShopFlashSaleApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.ITopPicksApi, Loscardos.ShopeeSdk.Core.Managers.Generated.TopPicksApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IVideoApi, Loscardos.ShopeeSdk.Core.Managers.Generated.VideoApi>();
        services.AddScoped<Loscardos.ShopeeSdk.Abstractions.Managers.Generated.IVoucherApi, Loscardos.ShopeeSdk.Core.Managers.Generated.VoucherApi>();
        services.AddSingleton<NoopShopeeRateLimiter>();
        services.AddSingleton<PartitionedShopeeRateLimiter>();
        services.AddSingleton<IShopeeRateLimiter>(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ShopeeOptions>>().Value;
            return options.EnableRateLimiting
                ? provider.GetRequiredService<PartitionedShopeeRateLimiter>()
                : provider.GetRequiredService<NoopShopeeRateLimiter>();
        });
        services.AddHttpClient<IShopeeClient, ShopeeClient>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ShopeeOptions>>().Value;
            httpClient.BaseAddress = options.ResolveBaseUrl();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        });

        return services;
    }
}
