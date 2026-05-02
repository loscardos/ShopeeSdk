using System.Text.Json.Serialization;

namespace ShopeeSdk.Generated.Logistics;

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
