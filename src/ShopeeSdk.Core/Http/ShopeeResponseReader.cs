using System.Text.Json;
using ShopeeSdk.Abstractions.Errors;
using ShopeeSdk.Abstractions.Http;

namespace ShopeeSdk.Core.Http;

public static class ShopeeResponseReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static ShopeeEnvelope<TResponse> ReadJsonEnvelope<TResponse>(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ShopeeSdkException("Shopee returned an empty response.");
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var requestId = root.TryGetProperty("request_id", out var requestIdElement) ? requestIdElement.GetString() : null;
        var error = root.TryGetProperty("error", out var errorElement) ? errorElement.GetString() : null;
        var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;

        if (root.TryGetProperty("response", out var responseElement))
        {
            var response = responseElement.ValueKind == JsonValueKind.Null
                ? default
                : responseElement.Deserialize<TResponse>(JsonOptions);
            return new ShopeeEnvelope<TResponse>(requestId, error, message, response);
        }

        var topLevel = root.Deserialize<TResponse>(JsonOptions);
        return new ShopeeEnvelope<TResponse>(requestId, error, message, topLevel);
    }
}
