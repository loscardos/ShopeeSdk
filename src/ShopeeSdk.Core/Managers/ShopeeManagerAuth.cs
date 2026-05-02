using ShopeeSdk.Core.Auth;

namespace ShopeeSdk.Core.Managers;

internal static class ShopeeManagerAuth
{
    public static async Task<Dictionary<string, object?>> CreateAuthQueryAsync(
        ShopeeTokenService tokenService,
        long shopId,
        CancellationToken cancellationToken)
    {
        var token = await tokenService.GetValidTokenAsync(shopId, null, cancellationToken);
        return new Dictionary<string, object?> { ["access_token"] = token.AccessToken };
    }
}
