using System.Text.Json.Serialization;

namespace ShopeeSdk.Generated.Product;

public sealed record GetItemListRequest(
    [property: JsonPropertyName("offset")] long Offset,
    [property: JsonPropertyName("page_size")] int PageSize,
    [property: JsonPropertyName("update_time_from")] long? UpdateTimeFrom = null,
    [property: JsonPropertyName("update_time_to")] long? UpdateTimeTo = null,
    [property: JsonPropertyName("item_status")] IReadOnlyList<string>? ItemStatus = null);

public sealed record GetItemListResponse(
    [property: JsonPropertyName("item")] IReadOnlyList<GetItemListItem> Item,
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("has_next_page")] bool HasNextPage,
    [property: JsonPropertyName("next_offset")] long? NextOffset);

public sealed record GetItemListItem(
    [property: JsonPropertyName("item_id")] long ItemId,
    [property: JsonPropertyName("item_status")] string ItemStatus,
    [property: JsonPropertyName("update_time")] long UpdateTime);

public sealed record UpdateStockRequest(
    [property: JsonPropertyName("item_id")] long ItemId,
    [property: JsonPropertyName("stock_list")] IReadOnlyList<UpdateStockItem> StockList);

public sealed record UpdateStockItem(
    [property: JsonPropertyName("model_id")] long ModelId,
    [property: JsonPropertyName("seller_stock")] IReadOnlyList<SellerStock> SellerStock);

public sealed record SellerStock(
    [property: JsonPropertyName("stock")] int Stock);

public sealed record UpdateStockResponse(
    [property: JsonPropertyName("success_list")] IReadOnlyList<long>? SuccessList,
    [property: JsonPropertyName("failure_list")] IReadOnlyList<long>? FailureList);
