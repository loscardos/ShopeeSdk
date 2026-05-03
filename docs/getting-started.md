# Getting Started

ShopeeSdk targets .NET 8 and is published as preview packages under the `Loscardos.ShopeeSdk.*` package IDs.

## Choose Packages

For ASP.NET Core applications, start with:

```bash
dotnet add package Loscardos.ShopeeSdk.Extensions.DependencyInjection --prerelease
```

Add EF Core token storage when you want the SDK to persist encrypted shop tokens:

```bash
dotnet add package Loscardos.ShopeeSdk.Storage.EntityFramework --prerelease
```

Use the lower-level packages directly when building custom hosts:

```bash
dotnet add package Loscardos.ShopeeSdk.Core --prerelease
dotnet add package Loscardos.ShopeeSdk.Generated --prerelease
```

## Configure ASP.NET Core

```csharp
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Extensions.DependencyInjection;
using ShopeeSdk.Storage.EntityFramework;

builder.Services.AddShopeeSdk(options =>
{
    options.PartnerId = builder.Configuration.GetValue<long>("Shopee:PartnerId");
    options.PartnerKey = builder.Configuration["Shopee:PartnerKey"]!;
    options.Region = ShopeeRegion.Global;
    options.EnableRateLimiting = true;
});

builder.Services.AddShopeeEntityFrameworkTokenStorage();
```

`AddShopeeSdk` registers:

- Shopee request signing and HTTP client.
- Auth API and token refresh service.
- Polished product, order, logistics, payment, and returns managers.
- Generated managers for the generated endpoint surface.
- Webhook verifier.
- Optional in-memory partitioned rate limiting.

## Configuration

`ShopeeOptions` supports:

| Option | Default | Notes |
| --- | --- | --- |
| `PartnerId` | `0` | Required for signed requests. |
| `PartnerKey` | empty | Required for signed requests. Store securely. |
| `Region` | `Global` | `Global`, `China`, `Brazil`, `TestGlobal`, or `TestChina`. |
| `BaseUrl` | region-derived | Override for custom Shopee-compatible environments. |
| `TokenRefreshSkew` | 5 minutes | Refresh tokens before expiry. |
| `RequestTimeout` | 30 seconds | Per request timeout. |
| `MaxTransientRetries` | 2 | Retry count for transient failures. |
| `EnableRateLimiting` | false | Enables SDK-side rate limiting. |
| `RateLimitPermitLimit` | 60 | Permit count per window. |
| `RateLimitQueueLimit` | 120 | Queue size when limiting is enabled. |
| `RateLimitWindow` | 1 minute | Rate limit window. |
| `UserAgent` | `InternalShopeeSdk/0.2` | Sent on SDK HTTP requests. |

## Next Steps

- Read [Authentication](auth.md) to wire the OAuth callback.
- Read [Token Storage](storage.md) before going beyond local testing.
- Read [Endpoint Usage](endpoints.md) to choose polished or generated managers.
