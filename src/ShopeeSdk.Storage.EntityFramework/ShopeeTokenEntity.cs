namespace ShopeeSdk.Storage.EntityFramework;

public sealed class ShopeeTokenEntity
{
    public long Id { get; set; }
    public long? ShopId { get; set; }
    public long? MerchantId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string? RequestId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
