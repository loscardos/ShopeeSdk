namespace ShopeeSdk.Generator;

public static class ShopeeTypeMapper
{
    public static string MapScalar(string schemaType)
    {
        return schemaType switch
        {
            "string" => "string?",
            "int" or "int32" or "integer" => "int?",
            "int64" or "long" => "long?",
            "float" or "double" or "decimal" => "decimal?",
            "boolean" or "bool" => "bool?",
            "string[]" => "IReadOnlyList<string>?",
            "int[]" or "int32[]" => "IReadOnlyList<int>?",
            "int64[]" => "IReadOnlyList<long>?",
            "file" => "ShopeeUploadFile?",
            _ => "JsonElement?"
        };
    }
}
