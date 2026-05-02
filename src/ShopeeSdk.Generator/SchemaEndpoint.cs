namespace ShopeeSdk.Generator;

public sealed record SchemaEndpoint(
    string ApiName,
    string ModuleName,
    string Path,
    string HttpMethod,
    IReadOnlyList<SchemaParameter> RequestParameters);

public sealed record SchemaParameter(
    string Name,
    string Type,
    bool Required);
