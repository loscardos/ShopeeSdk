using System.Text.Json.Serialization;

namespace ShopeeSdk.Generated.Order;

public sealed record GetOrderListRequest(
    [property: JsonPropertyName("time_range_field")] string TimeRangeField,
    [property: JsonPropertyName("time_from")] long TimeFrom,
    [property: JsonPropertyName("time_to")] long TimeTo,
    [property: JsonPropertyName("page_size")] int PageSize,
    [property: JsonPropertyName("cursor")] string? Cursor = null,
    [property: JsonPropertyName("order_status")] string? OrderStatus = null);

public sealed record GetOrderListResponse(
    [property: JsonPropertyName("more")] bool More,
    [property: JsonPropertyName("next_cursor")] string? NextCursor,
    [property: JsonPropertyName("order_list")] IReadOnlyList<OrderListItem> OrderList);

public sealed record OrderListItem(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("order_status")] string? OrderStatus);

public sealed record GetOrderDetailRequest(
    [property: JsonPropertyName("order_sn_list")] IReadOnlyList<string> OrderSnList,
    [property: JsonPropertyName("response_optional_fields")] string? ResponseOptionalFields = null);

public sealed record GetOrderDetailResponse(
    [property: JsonPropertyName("order_list")] IReadOnlyList<OrderDetail> OrderList);

public sealed record OrderDetail(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("order_status")] string OrderStatus,
    [property: JsonPropertyName("update_time")] long UpdateTime);

public sealed record GetShipmentListRequest(
    [property: JsonPropertyName("cursor")] string? Cursor,
    [property: JsonPropertyName("page_size")] int PageSize);

public sealed record GetShipmentListResponse(
    [property: JsonPropertyName("more")] bool More,
    [property: JsonPropertyName("next_cursor")] string? NextCursor,
    [property: JsonPropertyName("order_list")] IReadOnlyList<ShipmentOrder> OrderList);

public sealed record ShipmentOrder(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("package_number")] string? PackageNumber);

public sealed record SetNoteRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("note")] string Note);

public sealed record SetNoteResponse(
    [property: JsonPropertyName("order_sn")] string? OrderSn);
