using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopeeSdk.Abstractions.Auth;

namespace ShopeeSdk.Storage.EntityFramework.Tests;

public sealed class ShopeeEntityFrameworkStorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddShopeeEntityFrameworkTokenStorage_RegistersEncryptedTokenStore()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        services.AddDbContext<ShopeeTokenDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddShopeeEntityFrameworkTokenStorage();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IShopeeTokenStore>().Should().BeOfType<EfShopeeTokenStore>();
        provider.GetRequiredService<IShopeeTokenProtector>().Should().BeOfType<DataProtectionShopeeTokenProtector>();
    }
}
