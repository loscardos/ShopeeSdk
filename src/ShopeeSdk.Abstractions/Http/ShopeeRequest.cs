namespace ShopeeSdk.Abstractions.Http;

public enum ShopeeRequestContentKind
{
    Json,
    Multipart
}

public sealed record ShopeeRequest(
    ShopeeRequestMethod Method,
    string Path,
    IReadOnlyDictionary<string, object?> Query,
    object? Body,
    ShopeeRequestAuth? Auth,
    ShopeeRequestContentKind ContentKind = ShopeeRequestContentKind.Json);
