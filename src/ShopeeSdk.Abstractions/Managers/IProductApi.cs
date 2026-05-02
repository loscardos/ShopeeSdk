using ShopeeSdk.Generated.Product;

namespace ShopeeSdk.Abstractions.Managers;

public interface IProductApi
{
    Task<GetItemListResponse> GetItemListAsync(long shopId, GetItemListRequest request, CancellationToken cancellationToken);
    Task<GetItemBaseInfoResponse> GetItemBaseInfoAsync(long shopId, GetItemBaseInfoRequest request, CancellationToken cancellationToken);
    Task<GetModelListResponse> GetModelListAsync(long shopId, GetModelListRequest request, CancellationToken cancellationToken);
    Task<UpdateStockResponse> UpdateStockAsync(long shopId, UpdateStockRequest request, CancellationToken cancellationToken);
    Task<UpdatePriceResponse> UpdatePriceAsync(long shopId, UpdatePriceRequest request, CancellationToken cancellationToken);
}
