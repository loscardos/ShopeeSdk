using ShopeeSdk.Abstractions.Auth;

namespace ShopeeSdk.Abstractions.Managers;

public interface IAuthApi
{
    Uri BuildAuthorizationUrl(Uri redirectUri);
    Task<ShopeeToken> ExchangeCodeAsync(string code, long? shopId, long? merchantId, CancellationToken cancellationToken);
    Task<ShopeeToken> RefreshAsync(long? shopId, long? merchantId, CancellationToken cancellationToken);
}
