using Loscardos.ShopeeSdk.Generated.Logistics;

namespace Loscardos.ShopeeSdk.Abstractions.Managers;

public interface ILogisticsApi
{
    Task<GetShippingParameterResponse> GetShippingParameterAsync(long shopId, GetShippingParameterRequest request, CancellationToken cancellationToken);
    Task<ShipOrderResponse> ShipOrderAsync(long shopId, ShipOrderRequest request, CancellationToken cancellationToken);
    Task<GetTrackingInfoResponse> GetTrackingInfoAsync(long shopId, GetTrackingInfoRequest request, CancellationToken cancellationToken);
    Task<GetShippingDocumentParameterResponse> GetShippingDocumentParameterAsync(long shopId, GetShippingDocumentParameterRequest request, CancellationToken cancellationToken);
    Task<CreateShippingDocumentResponse> CreateShippingDocumentAsync(long shopId, CreateShippingDocumentRequest request, CancellationToken cancellationToken);
    Task<DownloadShippingDocumentResponse> DownloadShippingDocumentAsync(long shopId, DownloadShippingDocumentRequest request, CancellationToken cancellationToken);
    Task<DownloadShippingDocumentFile> DownloadShippingDocumentFileAsync(long shopId, DownloadShippingDocumentRequest request, CancellationToken cancellationToken);
}
