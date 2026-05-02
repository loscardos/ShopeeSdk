namespace ShopeeSdk.Storage.EntityFramework;

public sealed class PlainTextShopeeTokenProtector : IShopeeTokenProtector
{
    public string Protect(string value) => value;
    public string Unprotect(string value) => value;
}
