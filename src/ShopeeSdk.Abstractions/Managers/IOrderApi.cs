using ShopeeSdk.Generated.Order;

namespace ShopeeSdk.Abstractions.Managers;

public interface IOrderApi
{
    Task<GetOrderListResponse> GetOrderListAsync(long shopId, GetOrderListRequest request, CancellationToken cancellationToken);
    Task<GetOrderDetailResponse> GetOrderDetailAsync(long shopId, GetOrderDetailRequest request, CancellationToken cancellationToken);
    Task<GetShipmentListResponse> GetShipmentListAsync(long shopId, GetShipmentListRequest request, CancellationToken cancellationToken);
    Task<SetNoteResponse> SetNoteAsync(long shopId, SetNoteRequest request, CancellationToken cancellationToken);
}
