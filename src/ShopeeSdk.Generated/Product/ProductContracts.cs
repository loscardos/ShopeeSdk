using System.Text.Json;
using System.Text.Json.Serialization;

namespace Loscardos.ShopeeSdk.Generated.Product;

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

public sealed record GetItemBaseInfoRequest(
    [property: JsonPropertyName("item_id_list")] IReadOnlyList<long> ItemIdList,
    [property: JsonPropertyName("need_tax_info")] bool? NeedTaxInfo = null,
    [property: JsonPropertyName("need_complaint_policy")] bool? NeedComplaintPolicy = null);

public sealed record GetItemBaseInfoResponse(
    [property: JsonPropertyName("item_list")] IReadOnlyList<ItemBaseInfo> ItemList);

public sealed record ItemBaseInfo(
    [property: JsonPropertyName("item_id")] long ItemId,
    [property: JsonPropertyName("item_name")] string? ItemName,
    [property: JsonPropertyName("item_sku")] string? ItemSku,
    [property: JsonPropertyName("item_status")] string? ItemStatus);

public sealed record GetModelListRequest(
    [property: JsonPropertyName("item_id")] long ItemId);

public sealed record GetModelListResponse(
    [property: JsonPropertyName("model")] IReadOnlyList<ModelInfo> Model);

public sealed record ModelInfo(
    [property: JsonPropertyName("model_id")] long ModelId,
    [property: JsonPropertyName("model_sku")] string? ModelSku);

public sealed record UpdatePriceRequest(
    [property: JsonPropertyName("item_id")] long ItemId,
    [property: JsonPropertyName("price_list")] IReadOnlyList<UpdatePriceItem> PriceList);

public sealed record UpdatePriceItem(
    [property: JsonPropertyName("model_id")] long ModelId,
    [property: JsonPropertyName("original_price")] decimal OriginalPrice);

public sealed record UpdatePriceResponse(
    [property: JsonPropertyName("success_list")] IReadOnlyList<long>? SuccessList,
    [property: JsonPropertyName("failure_list")] IReadOnlyList<long>? FailureList);

internal sealed class ShopeeBooleanStringJsonConverter
    : JsonConverter<bool?>
{
    public override bool? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String
                when bool.TryParse(
                    reader.GetString(),
                    out var value) => value,
            JsonTokenType.Null => null,
            _ => throw new JsonException(
                "Shopee boolean value must be true, false, or a boolean string.")
        };

    public override void Write(
        Utf8JsonWriter writer,
        bool? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteBooleanValue(value.Value);
            return;
        }
        writer.WriteNullValue();
    }
}
