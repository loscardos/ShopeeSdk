namespace Loscardos.ShopeeSdk.Abstractions.Auth;

public interface IShopeeTokenStore
{
    Task<ShopeeToken?> GetAsync(long? shopId, long? merchantId, CancellationToken cancellationToken);
    Task StoreAsync(ShopeeToken token, CancellationToken cancellationToken);
    Task ClearAsync(long? shopId, long? merchantId, CancellationToken cancellationToken);
}
