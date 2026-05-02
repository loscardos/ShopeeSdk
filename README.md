# ShopeeSdk

ASP.NET-friendly Shopee Open API SDK for the internal omni-channel integration service.

## First Release Scope

- Shopee V2 request signing.
- OAuth authorization URL, token exchange, and token refresh.
- Multi-shop token store abstraction.
- Product, order, logistics, payment, and returns endpoint foundations.
- Dependency injection through `AddShopeeSdk`.

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
dotnet run --project samples/ShopeeSdk.SampleConsole -- get-orders
```

Use ignored local environment files such as `.token/shopee-sandbox.env` for sandbox credentials. Do not commit access tokens or refresh tokens.

## Production Pilot Checklist

See `docs/production-readiness.md` before using this SDK against live Shopee shops.

## Live Rollout

Before enabling production shops, read:

- `docs/production-readiness.md`
- `docs/endpoint-coverage.md`
- `docs/live-rollout-checklist.md`
