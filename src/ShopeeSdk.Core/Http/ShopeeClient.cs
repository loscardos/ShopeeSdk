using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Abstractions.Errors;
using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Abstractions.RateLimiting;
using Loscardos.ShopeeSdk.Core.Crypto;
using Loscardos.ShopeeSdk.Core.Time;
using Loscardos.ShopeeSdk.Generated.Common;

namespace Loscardos.ShopeeSdk.Core.Http;

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
                _logger.LogWarning(
                    exception,
                    "Retrying Shopee request {Path} after transient error {Error}",
                    request.Path,
                    exception.Error);
                await Task.Delay(RetryDelay(exception, attempt), cancellationToken);
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

    public async Task<(byte[] Content, string? ContentType, string? FileName)> SendForBytesAsync(
        ShopeeRequest request,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await SendForBytesOnceAsync(request, cancellationToken);
            }
            catch (ShopeeApiException exception) when (
                attempt < _options.MaxTransientRetries
                && (exception.Category is ShopeeErrorCategory.Transient or ShopeeErrorCategory.RateLimit))
            {
                _logger.LogWarning(
                    exception,
                    "Retrying Shopee binary request {Path} ShopId={ShopId} after transient error {Error} Category={Category}",
                    request.Path,
                    request.Auth?.ShopId,
                    exception.Error,
                    exception.Category);
                await Task.Delay(RetryDelay(exception, attempt), cancellationToken);
            }
            catch (HttpRequestException exception) when (attempt < _options.MaxTransientRetries)
            {
                _logger.LogWarning(exception, "Retrying Shopee binary request {Path} ShopId={ShopId} after HTTP failure", request.Path, request.Auth?.ShopId);
                await Task.Delay(_options.RetryBaseDelay * (attempt + 1), cancellationToken);
            }
            catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested && attempt < _options.MaxTransientRetries)
            {
                _logger.LogWarning(exception, "Retrying Shopee binary request {Path} ShopId={ShopId} after timeout", request.Path, request.Auth?.ShopId);
                await Task.Delay(_options.RetryBaseDelay * (attempt + 1), cancellationToken);
            }
        }
    }

    private async Task<(byte[] Content, string? ContentType, string? FileName)> SendForBytesOnceAsync(
        ShopeeRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureRateLimitPermitAsync(request, cancellationToken);

        using var httpRequest = BuildHttpRequest(request);
        _logger.LogInformation("Sending Shopee request {Method} {Path} ShopId={ShopId}", request.Method, request.Path, request.Auth?.ShopId);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.RequestTimeout);

        using var response = await httpClient.SendAsync(httpRequest, timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            var category = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                ? ShopeeErrorCategory.RateLimit
                : ShopeeErrorCategory.Transient;

            throw new ShopeeApiException(
                (int)response.StatusCode,
                null,
                $"http_{(int)response.StatusCode}",
                $"Shopee binary request failed with HTTP status {(int)response.StatusCode}.",
                category,
                request.Path,
                RetryAfter(response));
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        _logger.LogInformation(
            "Received Shopee binary response {Path} ShopId={ShopId} StatusCode={StatusCode} ContentType={ContentType} Length={Length}",
            request.Path,
            request.Auth?.ShopId,
            (int)response.StatusCode,
            response.Content.Headers.ContentType?.MediaType,
            bytes.Length);

        return (
            bytes,
            response.Content.Headers.ContentType?.MediaType,
            response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName);
    }

    private async Task<ShopeeEnvelope<TResponse>> SendOnceAsync<TResponse>(
        ShopeeRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureRateLimitPermitAsync(request, cancellationToken);

        using var httpRequest = BuildHttpRequest(request);
        _logger.LogInformation("Sending Shopee request {Method} {Path} ShopId={ShopId}", request.Method, request.Path, request.Auth?.ShopId);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.RequestTimeout);

        using var response = await httpClient.SendAsync(httpRequest, timeoutCts.Token);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        ShopeeEnvelope<TResponse> envelope;
        try
        {
            envelope = ShopeeResponseReader.ReadJsonEnvelope<TResponse>(payload);
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidOperationException)
        {
            var shape = DescribeJsonShape(payload);
            _logger.LogError(
                "Shopee response JSON contract invalid Path={Path} ShopId={ShopId} StatusCode={StatusCode} ExceptionType={ExceptionType} JsonPath={JsonPath} ResponseType={ResponseType} ContentType={ContentType} BodyLength={BodyLength} RootKind={RootKind} ResponseKind={ResponseKind} ErrorKind={ErrorKind} MessageKind={MessageKind}",
                request.Path,
                request.Auth?.ShopId,
                (int)response.StatusCode,
                exception.GetType().Name,
                (exception as JsonException)?.Path ?? "$",
                typeof(TResponse).Name,
                response.Content.Headers.ContentType?.MediaType ?? "unknown",
                payload.Length,
                shape.RootKind,
                shape.ResponseKind,
                shape.ErrorKind,
                shape.MessageKind);
            throw new ShopeeSdkException(
                $"Shopee response JSON contract invalid at {(exception as JsonException)?.Path ?? "$"}.",
                exception);
        }

        ShopeeErrorCategory? category = string.IsNullOrWhiteSpace(envelope.Error)
            ? null
            : ShopeeErrorClassifier.Classify(envelope.Error);

        _logger.LogInformation(
            "Received Shopee response {Path} ShopId={ShopId} StatusCode={StatusCode} RequestId={RequestId} Error={Error} Category={Category}",
            request.Path,
            request.Auth?.ShopId,
            (int)response.StatusCode,
            envelope.RequestId,
            envelope.Error,
            category);

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
                category ?? ShopeeErrorCategory.Unknown,
                request.Path,
                RetryAfter(response));
        }

        return envelope;
    }

    private static JsonShape DescribeJsonShape(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            return new(
                root.ValueKind.ToString(),
                PropertyKind(root, "response"),
                PropertyKind(root, "error"),
                PropertyKind(root, "message"));
        }
        catch (JsonException)
        {
            return new("Invalid", "Unknown", "Unknown", "Unknown");
        }
    }

    private static string PropertyKind(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object
        && root.TryGetProperty(name, out var value)
            ? value.ValueKind.ToString()
            : "Missing";

    private sealed record JsonShape(
        string RootKind,
        string ResponseKind,
        string ErrorKind,
        string MessageKind);

    private TimeSpan? RetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta)
            return delta > TimeSpan.Zero ? delta : null;
        if (retryAfter?.Date is not { } date)
            return null;
        var delay = date - DateTimeOffset.FromUnixTimeSeconds(clock.UnixTimeSeconds);
        return delay > TimeSpan.Zero ? delay : null;
    }

    private TimeSpan RetryDelay(ShopeeApiException exception, int attempt)
    {
        var configured = _options.RetryBaseDelay * (attempt + 1);
        return exception.RetryAfter is { } retryAfter && retryAfter > configured
            ? retryAfter
            : configured;
    }

    private async Task EnsureRateLimitPermitAsync(ShopeeRequest request, CancellationToken cancellationToken)
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
    }

    private HttpRequestMessage BuildHttpRequest(ShopeeRequest request)
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

        if (request.Auth is { ShopId: not null } shopAuth)
        {
            if (!query.TryGetValue("access_token", out var accessTokenValue) || accessTokenValue is null)
            {
                throw new ShopeeSdkException("Authenticated Shopee request requires access_token in the query.");
            }

            query["shop_id"] = shopAuth.ShopId.Value;
            signParts.Add(accessTokenValue.ToString()!);
            signParts.Add(shopAuth.ShopId.Value.ToString());
        }
        else if (request.Auth is { MerchantId: not null } merchantAuth)
        {
            if (!query.TryGetValue("access_token", out var accessTokenValue) || accessTokenValue is null)
            {
                throw new ShopeeSdkException("Authenticated Shopee merchant request requires access_token in the query.");
            }

            query["merchant_id"] = merchantAuth.MerchantId.Value;
            signParts.Add(accessTokenValue.ToString()!);
            signParts.Add(merchantAuth.MerchantId.Value.ToString());
        }

        query["sign"] = ShopeeSigner.Sign(_options.PartnerKey, signParts);

        var relative = path.TrimStart('/');
        var queryString = ShopeeQueryString.Build(query);
        var requestUri = new Uri(baseUrl, $"{relative}?{queryString}");
        var httpRequest = new HttpRequestMessage(ToHttpMethod(request.Method), requestUri);

        if (request.Body is not null)
        {
            httpRequest.Content = request.ContentKind == ShopeeRequestContentKind.Multipart
                ? BuildMultipartContent(request.Body)
                : JsonContent.Create(request.Body);
        }

        return httpRequest;
    }

    private static HttpContent BuildMultipartContent(object body)
    {
        var multipart = new MultipartFormDataContent();
        var values = body as IReadOnlyDictionary<string, object?>
            ?? body.GetType()
                .GetProperties()
                .ToDictionary(property => property.Name, property => property.GetValue(body), StringComparer.Ordinal);

        foreach (var (name, value) in values)
        {
            if (value is null)
            {
                continue;
            }

            if (value is ShopeeFilePart file)
            {
                AddFile(multipart, name, file);
                continue;
            }

            if (value is ShopeeUploadFile uploadFile)
            {
                AddFile(multipart, name, uploadFile);
                continue;
            }

            if (value is IEnumerable<ShopeeFilePart> files)
            {
                foreach (var item in files)
                {
                    AddFile(multipart, name, item);
                }

                continue;
            }

            if (value is IEnumerable<ShopeeUploadFile> uploadFiles)
            {
                foreach (var item in uploadFiles)
                {
                    AddFile(multipart, name, item);
                }

                continue;
            }

            if (value is byte[] bytes)
            {
                AddFile(multipart, name, new ShopeeFilePart(name, "application/octet-stream", bytes));
                continue;
            }

            multipart.Add(new StringContent(value.ToString()!), name);
        }

        return multipart;
    }

    private static void AddFile(MultipartFormDataContent multipart, string name, ShopeeFilePart file)
    {
        var content = new ByteArrayContent(file.Content);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        multipart.Add(content, name, file.FileName);
    }

    private static void AddFile(MultipartFormDataContent multipart, string name, ShopeeUploadFile file)
    {
        var content = new ByteArrayContent(file.Content);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        multipart.Add(content, name, file.FileName);
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
