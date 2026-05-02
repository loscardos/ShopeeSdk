using Microsoft.EntityFrameworkCore;
using ShopeeSdk.Abstractions.Auth;

namespace ShopeeSdk.Storage.EntityFramework;

public sealed class EfShopeeTokenStore(
    ShopeeTokenDbContext dbContext,
    IShopeeTokenProtector tokenProtector) : IShopeeTokenStore
{
    public async Task<ShopeeToken?> GetAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShopeeTokens
            .SingleOrDefaultAsync(token => token.ShopId == shopId && token.MerchantId == merchantId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new ShopeeToken(
            entity.ShopId,
            entity.MerchantId,
            tokenProtector.Unprotect(entity.AccessToken),
            tokenProtector.Unprotect(entity.RefreshToken),
            entity.ExpiresAt,
            entity.RequestId);
    }

    public async Task StoreAsync(ShopeeToken token, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShopeeTokens
            .SingleOrDefaultAsync(item => item.ShopId == token.ShopId && item.MerchantId == token.MerchantId, cancellationToken);

        if (entity is null)
        {
            entity = new ShopeeTokenEntity { ShopId = token.ShopId, MerchantId = token.MerchantId };
            dbContext.ShopeeTokens.Add(entity);
        }

        entity.AccessToken = tokenProtector.Protect(token.AccessToken);
        entity.RefreshToken = tokenProtector.Protect(token.RefreshToken);
        entity.ExpiresAt = token.ExpiresAt;
        entity.RequestId = token.RequestId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ShopeeTokens
            .SingleOrDefaultAsync(token => token.ShopId == shopId && token.MerchantId == merchantId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.ShopeeTokens.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
