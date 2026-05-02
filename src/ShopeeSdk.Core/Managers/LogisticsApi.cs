using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers;
using ShopeeSdk.Core.Auth;
using ShopeeSdk.Core.Validation;
using ShopeeSdk.Generated.Logistics;

namespace ShopeeSdk.Core.Managers;

public sealed class LogisticsApi(IShopeeClient client, ShopeeTokenService tokenService) : ILogisticsApi
{
    public async Task<GetShippingParameterResponse> GetShippingParameterAsync(long shopId, GetShippingParameterRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureRequiredString(request.OrderSn, "order_sn");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn"] = request.OrderSn;

        var envelope = await client.SendAsync<GetShippingParameterResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/get_shipping_parameter", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new GetShippingParameterResponse(null, null, null);
    }

    public async Task<ShipOrderResponse> ShipOrderAsync(long shopId, ShipOrderRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureRequiredString(request.OrderSn, "order_sn");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        var envelope = await client.SendAsync<ShipOrderResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/logistics/ship_order", query, request, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new ShipOrderResponse(null, null);
    }

    public async Task<GetTrackingInfoResponse> GetTrackingInfoAsync(long shopId, GetTrackingInfoRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureRequiredString(request.OrderSn, "order_sn");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn"] = request.OrderSn;
        query["package_number"] = request.PackageNumber;

        var envelope = await client.SendAsync<GetTrackingInfoResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/get_tracking_info", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new GetTrackingInfoResponse([]);
    }

    public async Task<GetShippingDocumentParameterResponse> GetShippingDocumentParameterAsync(long shopId, GetShippingDocumentParameterRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureRequiredString(request.OrderSn, "order_sn");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn"] = request.OrderSn;
        query["package_number"] = request.PackageNumber;

        var envelope = await client.SendAsync<GetShippingDocumentParameterResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/get_shipping_document_parameter", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new GetShippingDocumentParameterResponse(null);
    }

    public async Task<CreateShippingDocumentResponse> CreateShippingDocumentAsync(long shopId, CreateShippingDocumentRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureRequiredString(request.OrderSn, "order_sn");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        var envelope = await client.SendAsync<CreateShippingDocumentResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Post, "/logistics/create_shipping_document", query, request, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new CreateShippingDocumentResponse(null);
    }

    public async Task<DownloadShippingDocumentResponse> DownloadShippingDocumentAsync(long shopId, DownloadShippingDocumentRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureRequiredString(request.OrderSn, "order_sn");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn"] = request.OrderSn;
        query["package_number"] = request.PackageNumber;
        query["shipping_document_type"] = request.ShippingDocumentType;

        var envelope = await client.SendAsync<DownloadShippingDocumentResponse>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/download_shipping_document", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);
        return envelope.Response ?? new DownloadShippingDocumentResponse(null);
    }

    public async Task<DownloadShippingDocumentFile> DownloadShippingDocumentFileAsync(long shopId, DownloadShippingDocumentRequest request, CancellationToken cancellationToken)
    {
        ShopeeRequestValidator.EnsureRequiredString(request.OrderSn, "order_sn");

        var query = await ShopeeManagerAuth.CreateAuthQueryAsync(tokenService, shopId, cancellationToken);
        query["order_sn"] = request.OrderSn;
        query["package_number"] = request.PackageNumber;
        query["shipping_document_type"] = request.ShippingDocumentType;

        var file = await client.SendForBytesAsync(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/logistics/download_shipping_document", query, null, new ShopeeRequestAuth(shopId, null)),
            cancellationToken);

        return new DownloadShippingDocumentFile(file.Content, file.ContentType, file.FileName);
    }
}
