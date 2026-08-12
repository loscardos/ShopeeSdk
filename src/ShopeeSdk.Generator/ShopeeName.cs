using System.Text;

namespace Loscardos.ShopeeSdk.Generator;

public static class ShopeeName
{
    public static string ToPascalCase(string value)
    {
        var builder = new StringBuilder();
        var upperNext = true;

        foreach (var character in value)
        {
            if (!char.IsLetterOrDigit(character))
            {
                upperNext = true;
                continue;
            }

            builder.Append(upperNext ? char.ToUpperInvariant(character) : character);
            upperNext = false;
        }

        var result = builder.ToString();
        return result.Length == 0 ? "Value" : EscapeIdentifier(result);
    }

    public static string ToPropertyName(string value) => ToPascalCase(value);

    public static string ToMethodName(string path)
    {
        var action = path.Trim('/').Split('/')[1];
        return ToPascalCase(action) + "Async";
    }

    public static string ToRequestTypeName(SchemaEndpoint endpoint)
    {
        var action = endpoint.Path.Trim('/').Split('/')[1];
        return ToPascalCase(endpoint.ModuleKey) + ToPascalCase(action) + "Request";
    }

    public static string ToResponseTypeName(SchemaEndpoint endpoint)
    {
        var action = endpoint.Path.Trim('/').Split('/')[1];
        return ToPascalCase(endpoint.ModuleKey) + ToPascalCase(action) + "Response";
    }

    public static string ToManagerInterfaceName(string moduleKey) => "I" + ToPascalCase(moduleKey) + "Api";

    public static string ToManagerImplementationName(string moduleKey) => ToPascalCase(moduleKey) + "Api";

    private static string EscapeIdentifier(string value)
    {
        if (char.IsDigit(value[0]))
        {
            return "_" + value;
        }

        return value switch
        {
            "Class" or "Event" or "Object" or "String" => value + "Value",
            _ => value
        };
    }
}
