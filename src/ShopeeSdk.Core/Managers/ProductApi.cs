using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Generated.Product;

namespace ShopeeSdk.Core.Managers;

public sealed class ProductApi(IShopeeClient client, ShopeeTokenService tokenService) : IProductApi
{
    public async Task<GetItemListResponse> GetItemListAsync(long shopId, GetItemListRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["offset"] = request.Offset;
        query["page_size"] = request.PageSize;
        query["update_time_from"] = request.UpdateTimeFrom;
        query["update_time_to"] = request.UpdateTimeTo;
        query["item_status"] = request.ItemStatus;

        var envelope = await client.SendAsync<GetItemListResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/product/get_item_list", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new GetItemListResponse([], 0, false, null);
    }

    public async Task<UpdateStockResponse> UpdateStockAsync(long shopId, UpdateStockRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        var envelope = await client.SendAsync<UpdateStockResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/product/update_stock", query, request, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new UpdateStockResponse(null, null);
    }
}
