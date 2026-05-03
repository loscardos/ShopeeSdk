namespace ShopeeSdk.Generator;

public enum ShopeeSchemaHttpMethod
{
    Post,
    Get
}

public enum ShopeeSchemaAuthScope
{
    Public,
    Shop,
    Merchant
}

public enum ShopeeSchemaRequestContentKind
{
    Query,
    Json,
    Multipart
}

public sealed record SchemaEndpoint(
    string ApiName,
    string ModuleName,
    string ModuleKey,
    string Path,
    ShopeeSchemaHttpMethod HttpMethod,
    ShopeeSchemaAuthScope AuthScope,
    ShopeeSchemaRequestContentKind RequestContentKind,
    IReadOnlyList<SchemaParameter> RequestParameters,
    IReadOnlyList<SchemaParameter> ResponseParameters);

public sealed record SchemaParameter(
    string Name,
    string Type,
    bool Required,
    IReadOnlyList<SchemaParameter> Children);
