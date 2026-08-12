using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Abstractions.Managers;
using Loscardos.ShopeeSdk.Core.Auth;
using Loscardos.ShopeeSdk.Generated.Payment;

namespace Loscardos.ShopeeSdk.Core.Managers;

public sealed class PaymentApi(IShopeeClient client, ShopeeTokenService tokenService) : IPaymentApi
{
    public async Task<GetEscrowDetailResponse> GetEscrowDetailAsync(long shopId, GetEscrowDetailRequest request, CancellationToken cancellationToken)
    {
        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn"] = request.OrderSn;

        var envelope = await client.SendAsync<GetEscrowDetailResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/payment/get_escrow_detail", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new GetEscrowDetailResponse(request.OrderSn, null, null);
    }
}
