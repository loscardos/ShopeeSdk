using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Abstractions.Managers;
using Loscardos.ShopeeSdk.Core.Auth;
using Loscardos.ShopeeSdk.Core.Validation;
using Loscardos.ShopeeSdk.Generated.Returns;

namespace Loscardos.ShopeeSdk.Core.Managers;

public sealed class ReturnsApi(IShopeeClient client, ShopeeTokenService tokenService) : IReturnsApi
{
    public async Task<GetReturnListResponse> GetReturnListAsync(long shopId, GetReturnListRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsurePageSize(request.PageSize, 1, 100, "page_size");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["page_no"] = request.PageNo;
        query["page_size"] = request.PageSize;
        query["create_time_from"] = request.CreateTimeFrom;
        query["create_time_to"] = request.CreateTimeTo;

        var envelope = await client.SendAsync<GetReturnListResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/returns/get_return_list", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new GetReturnListResponse([], false);
    }
}
