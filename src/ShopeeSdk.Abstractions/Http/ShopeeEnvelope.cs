using System.Text.Json.Serialization;

namespace Loscardos.ShopeeSdk.Abstractions.Http;

public sealed record ShopeeEnvelope<T>(
    [property: JsonPropertyName("request_id")] string? RequestId,
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("response")] T? Response);
