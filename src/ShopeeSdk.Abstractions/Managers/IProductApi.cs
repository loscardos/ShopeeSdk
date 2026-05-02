using ShopeeSdk.Generated.Product;

namespace ShopeeSdk.Abstractions.Managers;

public interface IProductApi
{
    Task<GetItemListResponse> GetItemListAsync(long shopId, GetItemListRequest request, CancellationToken cancellationToken);
    Task<UpdateStockResponse> UpdateStockAsync(long shopId, UpdateStockRequest request, CancellationToken cancellationToken);
}
