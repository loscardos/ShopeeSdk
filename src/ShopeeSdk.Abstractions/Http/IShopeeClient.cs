namespace ShopeeSdk.Abstractions.Http;

public interface IShopeeClient
{
    Task<ShopeeEnvelope<TResponse>> SendAsync<TResponse>(
        ShopeeRequest request,
        CancellationToken cancellationToken);

    Task<(byte[] Content, string? ContentType, string? FileName)> SendForBytesAsync(
        ShopeeRequest request,
        CancellationToken cancellationToken);
}
