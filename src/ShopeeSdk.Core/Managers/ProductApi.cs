using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Abstractions.Managers;
using Loscardos.ShopeeSdk.Core.Auth;
using Loscardos.ShopeeSdk.Core.Validation;
using Loscardos.ShopeeSdk.Generated.Product;

namespace Loscardos.ShopeeSdk.Core.Managers;

public sealed class ProductApi(IShopeeClient client, ShopeeTokenService tokenService) : IProductApi
{
    public async Task<GetItemListResponse> GetItemListAsync(long shopId, GetItemListRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsurePageSize(request.PageSize, 1, 100, "page_size");

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

    public async Task<GetItemBaseInfoResponse> GetItemBaseInfoAsync(long shopId, GetItemBaseInfoRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureAny(request.ItemIdList, "item_id_list");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["item_id_list"] = string.Join(",", request.ItemIdList);
        query["need_tax_info"] = request.NeedTaxInfo;
        query["need_complaint_policy"] = request.NeedComplaintPolicy;

        var envelope = await client.SendAsync<GetItemBaseInfoResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/product/get_item_base_info", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new GetItemBaseInfoResponse([]);
    }

    public async Task<GetModelListResponse> GetModelListAsync(long shopId, GetModelListRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["item_id"] = request.ItemId;

        var envelope = await client.SendAsync<GetModelListResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/product/get_model_list", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new GetModelListResponse([]);
    }

    public async Task<UpdatePriceResponse> UpdatePriceAsync(long shopId, UpdatePriceRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        var envelope = await client.SendAsync<UpdatePriceResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/product/update_price", query, request, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new UpdatePriceResponse(null, null);
    }
}
