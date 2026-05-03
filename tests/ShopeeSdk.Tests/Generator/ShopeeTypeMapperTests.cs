using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class ShopeeTypeMapperTests
{
    [Theory]
    [InlineData("string", "string?")]
    [InlineData("int", "int?")]
    [InlineData("int32", "int?")]
    [InlineData("int64", "long?")]
    [InlineData("float", "decimal?")]
    [InlineData("double", "decimal?")]
    [InlineData("boolean", "bool?")]
    [InlineData("string[]", "IReadOnlyList<string>?")]
    [InlineData("int64[]", "IReadOnlyList<long>?")]
    [InlineData("file", "ShopeeUploadFile?")]
    public void MapScalar_MapsKnownShopeeTypes(string schemaType, string expected)
    {
        ShopeeTypeMapper.MapScalar(schemaType).Should().Be(expected);
    }

    [Fact]
    public void MapScalar_UsesJsonElementForUnknownSchemaTypes()
    {
        ShopeeTypeMapper.MapScalar("object").Should().Be("JsonElement?");
    }

    [Theory]
    [InlineData("string", true, "string")]
    [InlineData("string", false, "string?")]
    [InlineData("int64", true, "long")]
    [InlineData("int64", false, "long?")]
    [InlineData("boolean", true, "bool")]
    [InlineData("boolean", false, "bool?")]
    [InlineData("file", true, "ShopeeUploadFile")]
    [InlineData("file", false, "ShopeeUploadFile?")]
    public void MapScalar_UsesRequiredFlagForRequestTypes(string schemaType, bool required, string expected)
    {
        ShopeeTypeMapper.MapScalar(schemaType, required).Should().Be(expected);
    }
}
