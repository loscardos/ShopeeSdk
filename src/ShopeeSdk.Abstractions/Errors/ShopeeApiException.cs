namespace ShopeeSdk.Abstractions.Errors;

public sealed class ShopeeApiException : ShopeeSdkException
{
    public ShopeeApiException(int statusCode, string? requestId, string error, string? message)
        : base($"Shopee API error {error}: {message}")
    {
        StatusCode = statusCode;
        RequestId = requestId;
        Error = error;
        ApiMessage = message;
    }

    public int StatusCode { get; }
    public string? RequestId { get; }
    public string Error { get; }
    public string? ApiMessage { get; }
}
