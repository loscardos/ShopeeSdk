using Microsoft.Extensions.DependencyInjection;
using ShopeeSdk.Abstractions.Auth;

namespace ShopeeSdk.Storage.EntityFramework;

public static class ShopeeEntityFrameworkStorageServiceCollectionExtensions
{
    public static IServiceCollection AddShopeeEntityFrameworkTokenStorage(this IServiceCollection services)
    {
        services.AddSingleton<IShopeeTokenProtector, DataProtectionShopeeTokenProtector>();
        services.AddScoped<IShopeeTokenStore, EfShopeeTokenStore>();
        return services;
    }
}
