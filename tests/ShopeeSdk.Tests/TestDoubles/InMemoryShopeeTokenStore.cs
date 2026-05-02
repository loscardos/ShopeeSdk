using ShopeeSdk.Abstractions.Auth;

namespace ShopeeSdk.Tests.TestDoubles;

public sealed class InMemoryShopeeTokenStore : IShopeeTokenStore
{
    private readonly Dictionary<(long? ShopId, long? MerchantId), ShopeeToken> _tokens = new();

    public Task<ShopeeToken?> GetAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        _tokens.TryGetValue((shopId, merchantId), out var token);
        return Task.FromResult(token);
    }

    public Task StoreAsync(ShopeeToken token, CancellationToken cancellationToken)
    {
        _tokens[(token.ShopId, token.MerchantId)] = token;
        return Task.CompletedTask;
    }

    public Task ClearAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        _tokens.Remove((shopId, merchantId));
        return Task.CompletedTask;
    }
}
