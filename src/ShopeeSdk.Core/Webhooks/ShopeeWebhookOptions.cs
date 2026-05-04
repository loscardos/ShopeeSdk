namespace ShopeeSdk.Core.Webhooks;

public sealed class ShopeeWebhookOptions
{
    public TimeSpan TimestampTolerance { get; set; } = TimeSpan.FromMinutes(10);

    public string SignatureHeaderName { get; set; } = "authorization";
}
