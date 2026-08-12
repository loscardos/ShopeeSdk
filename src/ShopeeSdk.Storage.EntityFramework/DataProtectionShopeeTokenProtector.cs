using Microsoft.AspNetCore.DataProtection;

namespace Loscardos.ShopeeSdk.Storage.EntityFramework;

public sealed class DataProtectionShopeeTokenProtector : IShopeeTokenProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionShopeeTokenProtector(IDataProtectionProvider provider)
    {
        // Keep the original purpose string so existing encrypted tokens remain readable
        // across the public namespace and assembly branding migration.
        _protector = provider.CreateProtector("ShopeeSdk.Storage.EntityFramework.Tokens.v1");
    }

    public string Protect(string value) => _protector.Protect(value);

    public string Unprotect(string value) => _protector.Unprotect(value);
}
