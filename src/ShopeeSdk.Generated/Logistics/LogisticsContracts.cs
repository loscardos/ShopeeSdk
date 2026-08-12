using System.Text.Json.Serialization;

namespace Loscardos.ShopeeSdk.Generated.Logistics;

public sealed record GetShippingParameterRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn);

public sealed record GetShippingParameterResponse(
    [property: JsonPropertyName("info_needed")] IReadOnlyList<string>? InfoNeeded,
    [property: JsonPropertyName("dropoff")] object? Dropoff,
    [property: JsonPropertyName("pickup")] object? Pickup);

public sealed record ShipOrderRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber = null,
    [property: JsonPropertyName("pickup")] object? Pickup = null,
    [property: JsonPropertyName("dropoff")] object? Dropoff = null);

public sealed record ShipOrderResponse(
    [property: JsonPropertyName("order_sn")] string? OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber);

public sealed record GetTrackingInfoRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber = null);

public sealed record GetTrackingInfoResponse(
    [property: JsonPropertyName("tracking_info")] IReadOnlyList<TrackingInfo> TrackingInfo);

public sealed record TrackingInfo(
    [property: JsonPropertyName("update_time")] long UpdateTime,
    [property: JsonPropertyName("description")] string Description);

public sealed record GetShippingDocumentParameterRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber = null);

public sealed record GetShippingDocumentParameterResponse(
    [property: JsonPropertyName("suggest_shipping_document_type")] string? SuggestShippingDocumentType);

public sealed record CreateShippingDocumentRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber = null,
    [property: JsonPropertyName("shipping_document_type")] string? ShippingDocumentType = null);

public sealed record CreateShippingDocumentResponse(
    [property: JsonPropertyName("result_list")] IReadOnlyList<ShippingDocumentResult>? ResultList);

public sealed record ShippingDocumentResult(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber,
    [property: JsonPropertyName("fail_error")] string? FailError);

public sealed record DownloadShippingDocumentRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber = null,
    [property: JsonPropertyName("shipping_document_type")] string? ShippingDocumentType = null);

public sealed record DownloadShippingDocumentResponse(
    [property: JsonPropertyName("download_url")] string? DownloadUrl);

public sealed record DownloadShippingDocumentFile(
    byte[] Content,
    string? ContentType,
    string? FileName);
