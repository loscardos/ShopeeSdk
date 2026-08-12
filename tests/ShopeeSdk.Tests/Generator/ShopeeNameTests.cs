using FluentAssertions;
using Loscardos.ShopeeSdk.Generator;

namespace Loscardos.ShopeeSdk.Tests.Generator;

public sealed class ShopeeNameTests
{
    [Theory]
    [InlineData("get_item_list", "GetItemList")]
    [InlineData("media_space", "MediaSpace")]
    [InlineData("get_escrow_detail_batch", "GetEscrowDetailBatch")]
    [InlineData("ssp_id", "SspId")]
    public void ToPascalCase_ConvertsSnakeCase(string input, string expected)
    {
        ShopeeName.ToPascalCase(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("item_id", "ItemId")]
    [InlineData("item_id_list", "ItemIdList")]
    [InlineData("shop_id", "ShopId")]
    public void ToPropertyName_ConvertsSchemaField(string input, string expected)
    {
        ShopeeName.ToPropertyName(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("/product/get_category", "GetCategoryAsync")]
    [InlineData("/media/upload_image", "UploadImageAsync")]
    public void ToMethodName_ConvertsPathToAsyncMethod(string path, string expected)
    {
        ShopeeName.ToMethodName(path).Should().Be(expected);
    }
}
