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

## Production Pilot Checklist

See `docs/production-readiness.md` before using this SDK against live Shopee shops.
