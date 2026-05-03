# Endpoint Usage

The SDK exposes two manager surfaces.

## Polished Managers

Polished managers live in `ShopeeSdk.Abstractions.Managers` and focus on early ERP workflows:

- `IProductApi`
- `IOrderApi`
- `ILogisticsApi`
- `IPaymentApi`
- `IReturnsApi`
- `IAuthApi`

Use these when the method shape has been curated for common integration flows.

## Generated Managers

Generated managers live in `ShopeeSdk.Abstractions.Managers.Generated`. They provide broad coverage across the generated SDK endpoint surface. DTOs live in `ShopeeSdk.Generated.<Module>`.

Generated modules include:

- AccountHealth
- AddOnDeal
- Ads
- Ams
- BundleDeal
- Discount
- Fbs
- FirstMile
- FollowPrize
- GlobalProduct
- Livestream
- Logistics
- Media
- MediaSpace
- Merchant
- Order
- Payment
- Product
- Public
- Push
- Returns
- Sbs
- Shop
- ShopCategory
- ShopFlashSale
- TopPicks
- Video
- Voucher

## Example: Product Categories

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

## Example: Orders

```csharp
using ShopeeSdk.Generated.Order;
using GeneratedOrderApi = ShopeeSdk.Abstractions.Managers.Generated.IOrderApi;

var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

var response = await orderApi.GetOrderListAsync(
    shopId,
    new OrderGetOrderListRequest(
        TimeRangeField: "update_time",
        TimeFrom: now - 3600,
        TimeTo: now,
        PageSize: 50),
    cancellationToken);
```

## Coverage Guard

`tests/ShopeeSdk.Tests/Managers/ErpEndpointCoverageTests.cs` verifies the generated manager surface contains 425 generated methods and core ERP-critical methods such as product category, attribute, brand, order, logistics, payment, returns, merchant, and shop endpoints.

`tests/ShopeeSdk.Tests/PublicApi` guards public API shape drift.
