# Token Storage

Authenticated Shopee API calls require an access token for a shop or merchant. The SDK accesses tokens through `IShopeeTokenStore`.

## Interface

```csharp
using ShopeeSdk.Abstractions.Auth;

public interface IShopeeTokenStore
{
    Task<ShopeeToken?> GetAsync(long? shopId, long? merchantId, CancellationToken cancellationToken);
    Task StoreAsync(ShopeeToken token, CancellationToken cancellationToken);
    Task ClearAsync(long? shopId, long? merchantId, CancellationToken cancellationToken);
}
```

Use `shopId` for shop authorization and `merchantId` for merchant authorization.

## Entity Framework Storage

The `Loscardos.ShopeeSdk.Storage.EntityFramework` package provides:

- `ShopeeTokenDbContext`
- `EfShopeeTokenStore`
- `DataProtectionShopeeTokenProtector`
- `AddShopeeEntityFrameworkTokenStorage`

Register it with:

```csharp
using ShopeeSdk.Storage.EntityFramework;

builder.Services.AddShopeeEntityFrameworkTokenStorage();
```

The EF token store encrypts access tokens and refresh tokens through ASP.NET Data Protection.

## Production Checklist

- Persist Data Protection keys outside process memory.
- Back token storage with a database shared by all app instances.
- Never log access tokens or refresh tokens.
- Store Shopee shop ownership metadata separately from token values.
- Clear tokens when a seller disconnects the app.
- Rotate and reauthorize when Shopee returns unrecoverable auth errors.
