using Microsoft.Extensions.DependencyInjection;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Abstractions.RateLimiting;
using ShopeeSdk.Abstractions.Webhooks;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.Managers;
using ShopeeSdk.Core.RateLimiting;
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
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IAccountHealthApi, ShopeeSdk.Core.Managers.Generated.AccountHealthApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IAddOnDealApi, ShopeeSdk.Core.Managers.Generated.AddOnDealApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IAdsApi, ShopeeSdk.Core.Managers.Generated.AdsApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IAmsApi, ShopeeSdk.Core.Managers.Generated.AmsApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IBundleDealApi, ShopeeSdk.Core.Managers.Generated.BundleDealApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IDiscountApi, ShopeeSdk.Core.Managers.Generated.DiscountApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IFbsApi, ShopeeSdk.Core.Managers.Generated.FbsApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IFirstMileApi, ShopeeSdk.Core.Managers.Generated.FirstMileApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IFollowPrizeApi, ShopeeSdk.Core.Managers.Generated.FollowPrizeApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IGlobalProductApi, ShopeeSdk.Core.Managers.Generated.GlobalProductApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.ILivestreamApi, ShopeeSdk.Core.Managers.Generated.LivestreamApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.ILogisticsApi, ShopeeSdk.Core.Managers.Generated.LogisticsApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IMediaApi, ShopeeSdk.Core.Managers.Generated.MediaApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IMediaSpaceApi, ShopeeSdk.Core.Managers.Generated.MediaSpaceApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IMerchantApi, ShopeeSdk.Core.Managers.Generated.MerchantApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IOrderApi, ShopeeSdk.Core.Managers.Generated.OrderApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IPaymentApi, ShopeeSdk.Core.Managers.Generated.PaymentApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IProductApi, ShopeeSdk.Core.Managers.Generated.ProductApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IPublicApi, ShopeeSdk.Core.Managers.Generated.PublicApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IPushApi, ShopeeSdk.Core.Managers.Generated.PushApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IReturnsApi, ShopeeSdk.Core.Managers.Generated.ReturnsApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.ISbsApi, ShopeeSdk.Core.Managers.Generated.SbsApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IShopApi, ShopeeSdk.Core.Managers.Generated.ShopApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IShopCategoryApi, ShopeeSdk.Core.Managers.Generated.ShopCategoryApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IShopFlashSaleApi, ShopeeSdk.Core.Managers.Generated.ShopFlashSaleApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.ITopPicksApi, ShopeeSdk.Core.Managers.Generated.TopPicksApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IVideoApi, ShopeeSdk.Core.Managers.Generated.VideoApi>();
        services.AddScoped<ShopeeSdk.Abstractions.Managers.Generated.IVoucherApi, ShopeeSdk.Core.Managers.Generated.VoucherApi>();
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
