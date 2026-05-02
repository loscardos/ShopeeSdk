namespace ShopeeSdk.Abstractions.Http;

public interface IShopeeClient
{
    Task<ShopeeEnvelope<TResponse>> SendAsync<TResponse>(
        ShopeeRequest request,
        CancellationToken cancellationToken);
}
