using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Errors;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.RateLimiting;
using ShopeeSdk.Core.Crypto;
using ShopeeSdk.Core.Time;

namespace ShopeeSdk.Core.Http;

public sealed class ShopeeClient(
    HttpClient httpClient,
    IOptions<ShopeeOptions> options,
    IShopeeClock clock,
    IShopeeRateLimiter rateLimiter,
    ILogger<ShopeeClient> logger) : IShopeeClient
{
    private readonly ShopeeOptions _options = options.Value;
    private readonly ILogger<ShopeeClient> _logger = logger;

    public async Task<ShopeeEnvelope<TResponse>> SendAsync<TResponse>(
        ShopeeRequest request,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await SendOnceAsync<TResponse>(request, cancellationToken);
            }
            catch (ShopeeApiException exception) when (
                attempt < _options.MaxTransientRetries
                && (exception.Category is ShopeeErrorCategory.Transient or ShopeeErrorCategory.RateLimit))
            {
                await Task.Delay(_options.RetryBaseDelay * (attempt + 1), cancellationToken);
            }
            catch (HttpRequestException) when (attempt < _options.MaxTransientRetries)
            {
                await Task.Delay(_options.RetryBaseDelay * (attempt + 1), cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < _options.MaxTransientRetries)
            {
                await Task.Delay(_options.RetryBaseDelay * (attempt + 1), cancellationToken);
            }
        }
    }

    private async Task<ShopeeEnvelope<TResponse>> SendOnceAsync<TResponse>(
        ShopeeRequest request,
        CancellationToken cancellationToken)
    {
        if (!await rateLimiter.TryAcquireAsync(request, cancellationToken))
        {
            throw new ShopeeApiException(
                429,
                null,
                "rate_limit_exceeded",
                "Local Shopee SDK rate limit exceeded.",
                ShopeeErrorCategory.RateLimit,
                request.Path);
        }

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
            httpRequest.Content = JsonContent.Create(request.Body);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.RequestTimeout);

        using var response = await httpClient.SendAsync(httpRequest, timeoutCts.Token);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var envelope = ShopeeResponseReader.ReadJsonEnvelope<TResponse>(payload);

        if (!response.IsSuccessStatusCode && string.IsNullOrWhiteSpace(envelope.Error))
        {
            throw new ShopeeSdkException($"Shopee HTTP request failed with status {(int)response.StatusCode}.");
        }

        if (!string.IsNullOrWhiteSpace(envelope.Error))
        {
            throw new ShopeeApiException(
                (int)response.StatusCode,
                envelope.RequestId,
                envelope.Error,
                envelope.Message,
                ShopeeErrorClassifier.Classify(envelope.Error),
                request.Path);
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
}
