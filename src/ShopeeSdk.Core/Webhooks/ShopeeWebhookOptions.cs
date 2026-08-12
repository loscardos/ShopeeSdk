namespace Loscardos.ShopeeSdk.Core.Webhooks;

public sealed class ShopeeWebhookOptions
{
    public bool RequireFreshTimestamp { get; set; }

    public TimeSpan TimestampTolerance { get; set; } = TimeSpan.FromMinutes(10);

    public string SignatureHeaderName { get; set; } = "authorization";
}
