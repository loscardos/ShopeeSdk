namespace ShopeeSdk.Abstractions.Webhooks;

public sealed record ShopeeWebhookReceiveResult(
    bool Accepted,
    string Reason,
    ShopeeWebhookEnvelope? Envelope)
{
    public static ShopeeWebhookReceiveResult Reject(string reason)
        => new(false, reason, null);

    public static ShopeeWebhookReceiveResult Accept(ShopeeWebhookEnvelope envelope)
        => new(true, "accepted", envelope);
}
