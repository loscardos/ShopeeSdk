using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Loscardos.ShopeeSdk.Abstractions.Auth;

namespace Loscardos.ShopeeSdk.Storage.EntityFramework.Tests;

public sealed class EfShopeeTokenStoreTests
{
    [Fact]
    public async Task StoreAsync_UpsertsAndReadsTokenByShop()
    {
        var options = new DbContextOptionsBuilder<ShopeeTokenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ShopeeTokenDbContext(options);
        var store = new EfShopeeTokenStore(db, new PlainTextShopeeTokenProtector());
        var token = new ShopeeToken(600000, null, "access", "refresh", DateTimeOffset.UtcNow.AddHours(4), "req-1");

        await store.StoreAsync(token, CancellationToken.None);
        var loaded = await store.GetAsync(600000, null, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.AccessToken.Should().Be("access");
        loaded.RefreshToken.Should().Be("refresh");
    }
}
