using Loscardos.ShopeeSdk.Abstractions.Http;

namespace Loscardos.ShopeeSdk.Abstractions.Errors;

public sealed class ShopeeApiException : ShopeeSdkException
{
    public ShopeeApiException(
        int statusCode,
        string? requestId,
        string error,
        string? message,
        ShopeeErrorCategory category = ShopeeErrorCategory.Unknown,
        string? path = null)
        : base($"Shopee API error {error}: {message}")
    {
        StatusCode = statusCode;
        RequestId = requestId;
        Error = error;
        ApiMessage = message;
        Category = category;
        Path = path;
    }

    public int StatusCode { get; }
    public string? RequestId { get; }
    public string Error { get; }
    public string? ApiMessage { get; }
    public ShopeeErrorCategory Category { get; }
    public string? Path { get; }
}
