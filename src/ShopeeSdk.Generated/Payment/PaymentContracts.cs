using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShopeeSdk.Generated.Payment;

public sealed record GetEscrowDetailRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn);

public sealed record GetEscrowDetailResponse(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("buyer_total_amount")] decimal? BuyerTotalAmount,
    [property: JsonPropertyName("seller_income_breakdown")] object? SellerIncomeBreakdown);

internal sealed class PaymentGetPaymentMethodListResponseJsonConverter : JsonConverter<PaymentGetPaymentMethodListResponse>
{
    public override PaymentGetPaymentMethodListResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            var items = root.Deserialize<IReadOnlyList<PaymentGetPaymentMethodListResponseItem>>(options);
            return new PaymentGetPaymentMethodListResponse(Items: items);
        }

        var item = root.Deserialize<PaymentGetPaymentMethodListResponseItem>(options);
        return new PaymentGetPaymentMethodListResponse(
            item?.PaymentMethod,
            item?.Region,
            item is null ? null : [item]);
    }

    public override void Write(Utf8JsonWriter writer, PaymentGetPaymentMethodListResponse value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Items ?? [], options);
}
