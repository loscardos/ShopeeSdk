using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShopeeSdk.Abstractions.Webhooks;

public sealed record ShopeeWebhookEvent(
    [property: JsonPropertyName("shop_id")] long? ShopId,
    [property: JsonPropertyName("merchant_id")] long? MerchantId,
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("data")] JsonElement Data,
    [property: JsonPropertyName("timestamp")] long? Timestamp);
