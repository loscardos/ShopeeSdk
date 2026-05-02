using System.Collections.Concurrent;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Errors;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Time;

namespace ShopeeSdk.Core.Auth;

public sealed class ShopeeTokenService(
    IShopeeTokenStore tokenStore,
    IAuthApi authApi,
    Microsoft.Extensions.Options.IOptions<ShopeeOptions> options,
    IShopeeClock clock)
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> RefreshLocks = new();

    public async Task<ShopeeToken> GetValidTokenAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetAsync(shopId, merchantId, cancellationToken)
            ?? throw new ShopeeSdkException("Shopee token is missing. Authorize the shop before calling authenticated endpoints.");

        if (!NeedsRefresh(token))
        {
            return token;
        }

        var key = $"{shopId?.ToString() ?? "shop:none"}:{merchantId?.ToString() ?? "merchant:none"}";
        var refreshLock = RefreshLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            var latest = await tokenStore.GetAsync(shopId, merchantId, cancellationToken)
                ?? throw new ShopeeSdkException("Shopee token disappeared during refresh.");

            if (!NeedsRefresh(latest))
            {
                return latest;
            }

            return await authApi.RefreshAsync(shopId, merchantId, cancellationToken);
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private bool NeedsRefresh(ShopeeToken token)
    {
        return token.ExpiresAt <= clock.UtcNow.Add(options.Value.TokenRefreshSkew);
    }
}
