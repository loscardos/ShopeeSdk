using Microsoft.Extensions.DependencyInjection;
using Loscardos.ShopeeSdk.Abstractions.Auth;

namespace Loscardos.ShopeeSdk.Storage.EntityFramework;

public static class ShopeeEntityFrameworkStorageServiceCollectionExtensions
{
    public static IServiceCollection AddShopeeEntityFrameworkTokenStorage(this IServiceCollection services)
    {
        services.AddSingleton<IShopeeTokenProtector, DataProtectionShopeeTokenProtector>();
        services.AddScoped<IShopeeTokenStore, EfShopeeTokenStore>();
        return services;
    }
}
