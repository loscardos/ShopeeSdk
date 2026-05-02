using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Generated.Order;

namespace ShopeeSdk.Core.Managers;

public sealed class OrderApi(IShopeeClient client, ShopeeTokenService tokenService) : IOrderApi
{
    public async Task<GetOrderListResponse> GetOrderListAsync(long shopId, GetOrderListRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["time_range_field"] = request.TimeRangeField;
        query["time_from"] = request.TimeFrom;
        query["time_to"] = request.TimeTo;
        query["page_size"] = request.PageSize;
        query["cursor"] = request.Cursor;
        query["order_status"] = request.OrderStatus;

        var envelope = await client.SendAsync<GetOrderListResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/order/get_order_list", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new GetOrderListResponse(false, null, []);
    }

    public async Task<GetOrderDetailResponse> GetOrderDetailAsync(long shopId, GetOrderDetailRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn_list"] = string.Join(",", request.OrderSnList);
        query["response_optional_fields"] = request.ResponseOptionalFields;

        var envelope = await client.SendAsync<GetOrderDetailResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/order/get_order_detail", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new GetOrderDetailResponse([]);
    }
}
