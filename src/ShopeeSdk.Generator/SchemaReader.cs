using System.Text.Json;

namespace Loscardos.ShopeeSdk.Generator;

public sealed class SchemaReader
{
    public IReadOnlyList<SchemaEndpoint> ReadDirectory(string schemaDirectory)
    {
        var endpoints = new List<SchemaEndpoint>();

        foreach (var file in Directory.EnumerateFiles(schemaDirectory, "v2.*.json").OrderBy(path => path, StringComparer.Ordinal))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            var root = document.RootElement;
            if (!root.TryGetProperty("api_name", out var apiNameElement))
            {
                continue;
            }

            var apiName = apiNameElement.GetString()!.Trim();
            if (!apiName.StartsWith("v2.", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var moduleName = root.GetProperty("module_name").GetString()!.Trim();
            var moduleKey = apiName.Split('.')[1];
            var path = "/" + apiName.Replace("v2.", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(".", "/", StringComparison.Ordinal);
            var method = root.TryGetProperty("method", out var methodValue) && methodValue.GetInt32() == 2
                ? ShopeeSchemaHttpMethod.Get
                : ShopeeSchemaHttpMethod.Post;
            var authScope = ReadAuthScope(root);
            var requestParameters = ReadParameters(root, "request_params");
            var responseParameters = ReadParameters(root, "response_params");
            var contentKind = requestParameters.Any(parameter => string.Equals(parameter.Type, "file", StringComparison.OrdinalIgnoreCase))
                ? ShopeeSchemaRequestContentKind.Multipart
                : method == ShopeeSchemaHttpMethod.Get
                    ? ShopeeSchemaRequestContentKind.Query
                    : ShopeeSchemaRequestContentKind.Json;

            endpoints.Add(new SchemaEndpoint(
                apiName,
                moduleName,
                moduleKey,
                path,
                method,
                authScope,
                contentKind,
                requestParameters,
                responseParameters));
        }

        return endpoints;
    }

    private static ShopeeSchemaAuthScope ReadAuthScope(JsonElement root)
    {
        var apiType = root.TryGetProperty("api_type", out var apiTypeElement)
            ? apiTypeElement.GetString()
            : null;

        return apiType switch
        {
            "Shop" => ShopeeSchemaAuthScope.Shop,
            "Merchant" => ShopeeSchemaAuthScope.Merchant,
            _ => ShopeeSchemaAuthScope.Public
        };
    }

    private static IReadOnlyList<SchemaParameter> ReadParameters(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty("params", out var paramsElement)
            || !paramsElement.TryGetProperty(propertyName, out var parameters)
            || parameters.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return parameters.EnumerateArray().Select(ReadParameter).ToArray();
    }

    private static SchemaParameter ReadParameter(JsonElement parameter)
    {
        var children = parameter.TryGetProperty("children", out var childElement) && childElement.ValueKind == JsonValueKind.Array
            ? childElement.EnumerateArray().Select(ReadParameter).ToArray()
            : [];

        return new SchemaParameter(
            parameter.GetProperty("name").GetString()!,
            parameter.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? "object" : "object",
            IsRequired(parameter),
            children);
    }

    private static bool IsRequired(JsonElement parameter)
    {
        if (!parameter.TryGetProperty("required", out var required))
        {
            return false;
        }

        return required.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => string.Equals(required.GetString(), "True", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}
