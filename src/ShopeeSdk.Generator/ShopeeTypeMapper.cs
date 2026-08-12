namespace Loscardos.ShopeeSdk.Generator;

public static class ShopeeTypeMapper
{
    public static string MapScalar(string schemaType)
    {
        return MapScalar(schemaType, required: false);
    }

    public static string MapScalar(string schemaType, bool required)
    {
        var nullableSuffix = required ? string.Empty : "?";

        return schemaType switch
        {
            "string" => "string" + nullableSuffix,
            "int" or "int32" or "integer" => "int" + nullableSuffix,
            "int64" or "long" or "timestamp" => "long" + nullableSuffix,
            "float" or "double" or "decimal" => "decimal" + nullableSuffix,
            "boolean" or "bool" => "bool" + nullableSuffix,
            "string[]" => "IReadOnlyList<string>" + nullableSuffix,
            "int[]" or "int32[]" => "IReadOnlyList<int>" + nullableSuffix,
            "int64[]" => "IReadOnlyList<long>" + nullableSuffix,
            "float[]" or "double[]" or "decimal[]" => "IReadOnlyList<decimal>" + nullableSuffix,
            "file" => "ShopeeUploadFile" + nullableSuffix,
            _ => "JsonElement?"
        };
    }
}
