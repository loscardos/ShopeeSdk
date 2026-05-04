# ShopeeSdk

ASP.NET-friendly Shopee Open API SDK for .NET.

ShopeeSdk provides signed Shopee Open API requests, OAuth helpers, token refresh support, ASP.NET Core dependency injection, encrypted Entity Framework token storage, and generated endpoint clients for the Shopee API surface.

> Status: `0.1.0-preview`. The package is ready for sandbox validation and early integration work. Review behavior against your own Shopee app permissions before live rollout.

## Packages

| Package | Purpose |
| --- | --- |
| `Loscardos.ShopeeSdk.Extensions.DependencyInjection` | Recommended ASP.NET Core entrypoint. Registers the SDK, HTTP client, auth, polished managers, and generated managers. |
| `Loscardos.ShopeeSdk.Core` | Core signing, HTTP pipeline, retries, rate limiting, token refresh, and manager implementations. |
| `Loscardos.ShopeeSdk.Abstractions` | Public contracts for configuration, token stores, managers, errors, HTTP requests, and webhooks. |
| `Loscardos.ShopeeSdk.Generated` | Generated Shopee request and response DTO contracts. |
| `Loscardos.ShopeeSdk.Storage.EntityFramework` | EF Core token store with ASP.NET Data Protection encryption. |

## Install

For an ASP.NET Core application:

```bash
dotnet add package Loscardos.ShopeeSdk.Extensions.DependencyInjection --prerelease
dotnet add package Loscardos.ShopeeSdk.Storage.EntityFramework --prerelease
```

For lower-level consumers:

```bash
dotnet add package Loscardos.ShopeeSdk.Core --prerelease
dotnet add package Loscardos.ShopeeSdk.Generated --prerelease
```

## Quick Start

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

Applications must register an `IShopeeTokenStore`. The EF storage package is provided for ASP.NET Core apps; custom services can implement `IShopeeTokenStore` directly.

## Auth Flow

```csharp
using ShopeeSdk.Abstractions.Managers;

var authorizeUrl = authApi.BuildAuthorizationUrl(new Uri("https://example.com/shopee/callback"));
```

After Shopee redirects back with `code` and `shop_id`:

```csharp
var token = await authApi.ExchangeCodeAsync(
    code,
    shopId,
    merchantId: null,
    cancellationToken);
```

Persisted tokens are used by shop-scoped generated managers and refreshed through `ShopeeTokenService` when needed.

## Calling Endpoints

The SDK exposes two API surfaces:

- Polished managers for common ERP workflows: product, order, logistics, payment, and returns.
- Generated managers for full SDK endpoint coverage. Generated managers live under `ShopeeSdk.Abstractions.Managers.Generated` and use DTOs from `ShopeeSdk.Generated.<Module>`.

Example using a generated product manager:

```csharp
using ShopeeSdk.Generated.Product;
using GeneratedProductApi = ShopeeSdk.Abstractions.Managers.Generated.IProductApi;

public sealed class CategorySync(GeneratedProductApi productApi)
{
    public Task<ProductGetCategoryResponse> GetCategoriesAsync(
        long shopId,
        CancellationToken cancellationToken)
    {
        return productApi.GetCategoryAsync(
            shopId,
            new ProductGetCategoryRequest(Language: "en"),
            cancellationToken);
    }
}
```

## Sample Console

The sample console can generate authorization URLs, exchange sandbox codes, refresh tokens, and run read-only sandbox checks.

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- auth-url
dotnet run --project samples/ShopeeSdk.SampleConsole -- exchange-code
dotnet run --project samples/ShopeeSdk.SampleConsole -- refresh-token
dotnet run --project samples/ShopeeSdk.SampleConsole -- refresh-token --update-env .token/shopee-sandbox.env
dotnet run --project samples/ShopeeSdk.SampleConsole -- get-orders
dotnet run --project samples/ShopeeSdk.SampleConsole -- sandbox-check
```

Use ignored local files such as `.token/shopee-sandbox.env` for sandbox credentials. Do not commit access tokens, refresh tokens, partner keys, or shop credentials.

## Documentation

- [Getting Started](docs/getting-started.md)
- [Authentication](docs/auth.md)
- [Endpoint Usage](docs/endpoints.md)
- [Token Storage](docs/storage.md)
- [Sandbox Validation](docs/sandbox.md)
- [Packaging and Verification](docs/packaging.md)

## Development

```bash
dotnet restore ShopeeSdk.sln
dotnet build ShopeeSdk.sln --no-restore
dotnet test ShopeeSdk.sln --no-build
./scripts/verify-generated.sh
./scripts/pack-local.sh
./scripts/inspect-nuget.sh
./scripts/test-local-nuget.sh
```

The CI workflow runs restore, build, test, pack, local NuGet install validation, and generated SDK surface verification.

## License

MIT.
