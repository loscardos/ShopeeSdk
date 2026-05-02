namespace ShopeeSdk.Abstractions.Http;

public sealed record ShopeeRequest(
    ShopeeRequestMethod Method,
    string Path,
    IReadOnlyDictionary<string, object?> Query,
    object? Body,
    ShopeeRequestAuth? Auth);
