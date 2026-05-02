using System.Text.Json;

namespace ShopeeSdk.Generator;

public sealed class SchemaReader
{
    public IReadOnlyList<SchemaEndpoint> ReadDirectory(string schemaDirectory)
    {
        var endpoints = new List<SchemaEndpoint>();

        foreach (var file in Directory.EnumerateFiles(schemaDirectory, "*.json"))
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

            var moduleName = root.GetProperty("module_name").GetString()!;
            var path = "/" + apiName.Replace("v2.", string.Empty).Replace(".", "/");
            var method = root.TryGetProperty("method", out var methodValue) && methodValue.GetInt32() == 2 ? "GET" : "POST";
            var parameters = new List<SchemaParameter>();

            if (root.TryGetProperty("params", out var paramsElement)
                && paramsElement.TryGetProperty("request_params", out var requestParams)
                && requestParams.ValueKind == JsonValueKind.Array)
            {
                foreach (var parameter in requestParams.EnumerateArray())
                {
                    parameters.Add(new SchemaParameter(
                        parameter.GetProperty("name").GetString()!,
                        parameter.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? "object" : "object",
                        IsRequired(parameter)));
                }
            }

            endpoints.Add(new SchemaEndpoint(apiName, moduleName, path, method, parameters));
        }

        return endpoints;
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
