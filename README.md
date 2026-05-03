# ShopeeSdk

ASP.NET-friendly Shopee Open API SDK for .NET.

## First Release Scope

- Shopee V2 request signing.
- OAuth authorization URL, token exchange, and token refresh.
- Multi-shop token store abstraction.
- Product, order, logistics, payment, and returns endpoint foundations.
- Generated full endpoint coverage from the SDK schema snapshot.
- Dependency injection through `AddShopeeSdk`.

## Endpoint Coverage

The SDK ships two API surfaces:

- Polished managers in `ShopeeSdk.Abstractions.Managers` for auth and the first ERP workflows.
- Generated managers in `ShopeeSdk.Abstractions.Managers.Generated` for full Shopee schema coverage.

Generated managers are grouped by Shopee module and use request/response contracts from `ShopeeSdk.Generated.<Module>`.

Example:

```csharp
using ShopeeSdk.Abstractions.Managers.Generated;
using ShopeeSdk.Generated.Product;

var categories = await productApi.GetCategoryAsync(
    shopId,
    new ProductGetCategoryRequest(Language: "en"),
    cancellationToken);
```

## ASP.NET Registration

```csharp
services.AddShopeeSdk(options =>
{
    options.PartnerId = configuration.GetValue<long>("Shopee:PartnerId");
    options.PartnerKey = configuration["Shopee:PartnerKey"]!;
    options.Region = ShopeeRegion.Global;
});
```

Applications must register an `IShopeeTokenStore` implementation backed by encrypted persistent storage.

## Order Sync Example

```csharp
var orders = await orderApi.GetOrderListAsync(
    shopId,
    new GetOrderListRequest(
        TimeRangeField: "update_time",
        TimeFrom: fromUnix,
        TimeTo: toUnix,
        PageSize: 50),
    cancellationToken);
```

## Sample Console

The sample console can generate an authorization URL, exchange a sandbox code, and call a read-only order endpoint.

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- auth-url
dotnet run --project samples/ShopeeSdk.SampleConsole -- exchange-code
dotnet run --project samples/ShopeeSdk.SampleConsole -- refresh-token
dotnet run --project samples/ShopeeSdk.SampleConsole -- get-orders
dotnet run --project samples/ShopeeSdk.SampleConsole -- sandbox-check
```

Use ignored local environment files such as `.token/shopee-sandbox.env` for sandbox credentials. Do not commit access tokens or refresh tokens.

## Production Hardening

Before publishing a preview package, run the local hardening checks:

```bash
./scripts/verify-generated.sh
dotnet test ShopeeSdk.sln --no-build
dotnet pack ShopeeSdk.sln --no-build --output artifacts
./scripts/inspect-nuget.sh
./scripts/test-local-nuget.sh
```

Generated endpoint coverage is guarded by `tests/ShopeeSdk.Tests/Managers/ErpEndpointCoverageTests.cs`, and public API type drift is guarded by `tests/ShopeeSdk.Tests/PublicApi/public-api-baseline.txt`.
