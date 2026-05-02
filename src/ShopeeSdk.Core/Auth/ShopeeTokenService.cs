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
    public async Task<ShopeeToken> GetValidTokenAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetAsync(shopId, merchantId, cancellationToken)
            ?? throw new ShopeeSdkException("Shopee token is missing. Authorize the shop before calling authenticated endpoints.");

        if (token.ExpiresAt <= clock.UtcNow.Add(options.Value.TokenRefreshSkew))
        {
            return await authApi.RefreshAsync(shopId, merchantId, cancellationToken);
        }

        return token;
    }
}
