using System.Text.Json;
using System.Text.Json.Serialization;

namespace Loscardos.ShopeeSdk.Generated.Ads;

internal sealed class AdsGetRecommendedItemListResponseJsonConverter : JsonConverter<AdsGetRecommendedItemListResponse>
{
    public override AdsGetRecommendedItemListResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            var items = root.Deserialize<IReadOnlyList<AdsGetRecommendedItemListResponseItem>>(options);
            return new AdsGetRecommendedItemListResponse(Items: items);
        }

        var item = root.Deserialize<AdsGetRecommendedItemListResponseItem>(options);
        return new AdsGetRecommendedItemListResponse(
            item?.ItemId,
            item?.ItemStatusList,
            item?.SkuTagList,
            item?.OngoingAdTypeList,
            item is null ? null : [item]);
    }

    public override void Write(Utf8JsonWriter writer, AdsGetRecommendedItemListResponse value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Items ?? [], options);
}
