using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Generated.Logistics;

namespace ShopeeSdk.Core.Managers;

public sealed class LogisticsApi(IShopeeClient client, ShopeeTokenService tokenService) : ILogisticsApi
{
    public async Task<GetShippingParameterResponse> GetShippingParameterAsync(long shopId, GetShippingParameterRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn"] = request.OrderSn;

        var envelope = await client.SendAsync<GetShippingParameterResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/get_shipping_parameter", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new GetShippingParameterResponse(null, null, null);
    }

    public async Task<ShipOrderResponse> ShipOrderAsync(long shopId, ShipOrderRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        var envelope = await client.SendAsync<ShipOrderResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/logistics/ship_order", query, request, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new ShipOrderResponse(null, null);
    }
}
