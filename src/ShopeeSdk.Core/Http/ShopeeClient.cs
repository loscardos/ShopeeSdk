using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Errors;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Core.Crypto;
using ShopeeSdk.Core.Time;

namespace ShopeeSdk.Core.Http;

public sealed class ShopeeClient(
    HttpClient httpClient,
    IOptions<ShopeeOptions> options,
    IShopeeClock clock) : IShopeeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ShopeeOptions _options = options.Value;

    public async Task<ShopeeEnvelope<TResponse>> SendAsync<TResponse>(
        ShopeeRequest request,
        CancellationToken cancellationToken)
    {
        var timestamp = clock.UnixTimeSeconds;
        var baseUrl = _options.ResolveBaseUrl();
        var path = request.Path.StartsWith('/') ? request.Path : "/" + request.Path;
        var apiPath = baseUrl.AbsolutePath.TrimEnd('/') + path;

        var query = new Dictionary<string, object?>(request.Query)
        {
            ["partner_id"] = _options.PartnerId,
            ["timestamp"] = timestamp
        };

        var signParts = new List<string>
        {
            _options.PartnerId.ToString(),
            apiPath,
            timestamp.ToString()
        };

        if (request.Auth is { ShopId: not null } auth)
        {
            if (!query.TryGetValue("access_token", out var accessTokenValue) || accessTokenValue is null)
            {
                throw new ShopeeSdkException("Authenticated Shopee request requires access_token in the query.");
            }

            query["shop_id"] = auth.ShopId.Value;
            signParts.Add(accessTokenValue.ToString()!);
            signParts.Add(auth.ShopId.Value.ToString());
        }

        query["sign"] = ShopeeSigner.Sign(_options.PartnerKey, signParts);

        var relative = path.TrimStart('/');
        var queryString = ShopeeQueryString.Build(query);
        var requestUri = new Uri(baseUrl, $"{relative}?{queryString}");
        using var httpRequest = new HttpRequestMessage(ToHttpMethod(request.Method), requestUri);

        if (request.Body is not null)
        {
            httpRequest.Content = JsonContent.Create(request.Body, options: JsonOptions);
        }

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var envelope = NormalizeEnvelope<TResponse>(payload);

        if (!response.IsSuccessStatusCode && string.IsNullOrWhiteSpace(envelope.Error))
        {
            throw new ShopeeSdkException($"Shopee HTTP request failed with status {(int)response.StatusCode}.");
        }

        if (!string.IsNullOrWhiteSpace(envelope.Error))
        {
            throw new ShopeeApiException((int)response.StatusCode, envelope.RequestId, envelope.Error, envelope.Message);
        }

        return envelope;
    }

    private static HttpMethod ToHttpMethod(ShopeeRequestMethod method)
    {
        return method switch
        {
            ShopeeRequestMethod.Get => HttpMethod.Get,
            ShopeeRequestMethod.Post => HttpMethod.Post,
            _ => throw new ShopeeSdkException($"Unsupported Shopee request method: {method}")
        };
    }

    private static ShopeeEnvelope<TResponse> NormalizeEnvelope<TResponse>(string payload)
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
