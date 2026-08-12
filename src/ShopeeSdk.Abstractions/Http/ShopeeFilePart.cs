namespace Loscardos.ShopeeSdk.Abstractions.Http;

public sealed record ShopeeFilePart(
    string FileName,
    string ContentType,
    byte[] Content);
