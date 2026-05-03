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
}
