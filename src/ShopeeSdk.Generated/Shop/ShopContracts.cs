using System.Text.Json;
using System.Text.Json.Serialization;

namespace Loscardos.ShopeeSdk.Generated.Shop;

internal sealed class ShopGetWarehouseDetailResponseJsonConverter
    : JsonConverter<ShopGetWarehouseDetailResponse>
{
    public override ShopGetWarehouseDetailResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var items = root.ValueKind == JsonValueKind.Array
            ? root.Deserialize<IReadOnlyList<ShopGetWarehouseDetailResponseItem>>(options)
            : DeserializeSingle(root, options);
        var first = items?.FirstOrDefault();
        return new ShopGetWarehouseDetailResponse(
            first?.WarehouseId,
            first?.WarehouseName,
            first?.WarehouseType,
            first?.LocationId,
            first?.AddressId,
            first?.Region,
            first?.State,
            first?.City,
            first?.Address,
            first?.Zipcode,
            first?.District,
            first?.Town,
            first?.StateCode,
            first?.HolidayModeState,
            items);
    }

    public override void Write(
        Utf8JsonWriter writer,
        ShopGetWarehouseDetailResponse value,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.Items ?? [], options);

    private static IReadOnlyList<ShopGetWarehouseDetailResponseItem> DeserializeSingle(
        JsonElement root,
        JsonSerializerOptions options)
    {
        var item = root.Deserialize<ShopGetWarehouseDetailResponseItem>(options);
        return item is null ? [] : [item];
    }
}
