namespace Loscardos.ShopeeSdk.Abstractions.Webhooks;

public sealed record ShopeeWebhookEnvelope(
    string Marketplace,
    string RawBody,
    string Signature,
    string IdempotencyKey,
    DateTimeOffset ReceivedAt,
    ShopeeWebhookEvent Event);
