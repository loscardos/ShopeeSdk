using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Generated.Payment;

namespace ShopeeSdk.Core.Managers;

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
