using System.Text.Json.Serialization;

namespace ShopeeSdk.Generated.Returns;

public sealed record GetReturnListRequest(
    [property: JsonPropertyName("page_no")] int PageNo,
    [property: JsonPropertyName("page_size")] int PageSize,
    [property: JsonPropertyName("create_time_from")] long? CreateTimeFrom = null,
    [property: JsonPropertyName("create_time_to")] long? CreateTimeTo = null);

public sealed record GetReturnListResponse(
    [property: JsonPropertyName("returnsn_list")] IReadOnlyList<string> ReturnSnList,
    [property: JsonPropertyName("more")] bool More);
