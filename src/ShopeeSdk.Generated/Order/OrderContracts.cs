using System.Text.Json;
using System.Text.Json.Serialization;

namespace Loscardos.ShopeeSdk.Generated.Order;

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

internal sealed class ShopeeFlexibleIdentifierJsonConverter
    : JsonConverter<string?>
{
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.StartArray => ReadSingletonArray(ref reader),
            JsonTokenType.Null => null,
            _ => throw new JsonException(
                "Shopee identifier must be a string, number, or singleton array.")
        };

    public override void Write(
        Utf8JsonWriter writer,
        string? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value);
    }

    private static string ReadNumber(ref Utf8JsonReader reader)
    {
        using var value = JsonDocument.ParseValue(ref reader);
        return value.RootElement.GetRawText();
    }

    private static string ReadSingletonArray(ref Utf8JsonReader reader)
    {
        using var value = JsonDocument.ParseValue(ref reader);
        var items = value.RootElement.EnumerateArray().ToArray();
        if (items.Length != 1)
        {
            throw new JsonException(
                "Shopee identifier array must contain exactly one value.");
        }
        return items[0].ValueKind switch
        {
            JsonValueKind.String => items[0].GetString()!,
            JsonValueKind.Number => items[0].GetRawText(),
            _ => throw new JsonException(
                "Shopee identifier array value must be a string or number.")
        };
    }
}
