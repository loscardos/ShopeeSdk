namespace ShopeeSdk.Abstractions.Auth;

public sealed record ShopeeToken(
    long? ShopId,
    long? MerchantId,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    string? RequestId);
