using ShopeeSdk.Generated.Order;

namespace ShopeeSdk.Abstractions.Managers;

public interface IOrderApi
{
    Task<GetOrderListResponse> GetOrderListAsync(long shopId, GetOrderListRequest request, CancellationToken cancellationToken);
    Task<GetOrderDetailResponse> GetOrderDetailAsync(long shopId, GetOrderDetailRequest request, CancellationToken cancellationToken);
}
