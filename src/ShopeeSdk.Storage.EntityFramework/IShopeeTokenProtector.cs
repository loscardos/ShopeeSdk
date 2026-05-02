namespace ShopeeSdk.Storage.EntityFramework;

public interface IShopeeTokenProtector
{
    string Protect(string value);
    string Unprotect(string value);
}
