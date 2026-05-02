using ShopeeSdk.Generated.Logistics;

namespace ShopeeSdk.Abstractions.Managers;

public interface ILogisticsApi
{
    Task<GetShippingParameterResponse> GetShippingParameterAsync(long shopId, GetShippingParameterRequest request, CancellationToken cancellationToken);
    Task<ShipOrderResponse> ShipOrderAsync(long shopId, ShipOrderRequest request, CancellationToken cancellationToken);
}
