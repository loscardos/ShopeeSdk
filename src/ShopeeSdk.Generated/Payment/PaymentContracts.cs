using System.Text.Json.Serialization;

namespace ShopeeSdk.Generated.Payment;

public sealed record GetEscrowDetailRequest(
    [property: JsonPropertyName("order_sn")] string OrderSn);

public sealed record GetEscrowDetailResponse(
    [property: JsonPropertyName("order_sn")] string OrderSn,
    [property: JsonPropertyName("buyer_total_amount")] decimal? BuyerTotalAmount,
    [property: JsonPropertyName("seller_income_breakdown")] object? SellerIncomeBreakdown);
