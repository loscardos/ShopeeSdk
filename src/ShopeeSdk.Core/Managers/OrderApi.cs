using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Abstractions.Managers;
using Loscardos.ShopeeSdk.Core.Auth;
using Loscardos.ShopeeSdk.Core.Validation;
using Loscardos.ShopeeSdk.Generated.Order;

namespace Loscardos.ShopeeSdk.Core.Managers;

public sealed class OrderApi(IShopeeClient client, ShopeeTokenService tokenService) : IOrderApi
{
    public async Task<GetOrderListResponse> GetOrderListAsync(long shopId, GetOrderListRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsurePageSize(request.PageSize, 1, 100, "page_size");
        ShopeeRequestValidator.EnsureTimeRange(request.TimeFrom, request.TimeTo, TimeSpan.FromDays(15), "order time range");

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

    public async Task<GetShipmentListResponse> GetShipmentListAsync(long shopId, GetShipmentListRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["cursor"] = request.Cursor;
        query["page_size"] = request.PageSize;

        var envelope = await client.SendAsync<GetShipmentListResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/order/get_shipment_list", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new GetShipmentListResponse(false, null, []);
    }

    public async Task<SetNoteResponse> SetNoteAsync(long shopId, SetNoteRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        var envelope = await client.SendAsync<SetNoteResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/order/set_note", query, request, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return envelope.Response ?? new SetNoteResponse(request.OrderSn);
    }
}
