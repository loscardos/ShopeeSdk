using FluentAssertions;
using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Core.Http;

namespace Loscardos.ShopeeSdk.Tests.Http;

public sealed class ShopeeErrorClassifierTests
{
    [Theory]
    [InlineData("error_auth", ShopeeErrorCategory.Authentication)]
    [InlineData("invalid_access_token", ShopeeErrorCategory.Authentication)]
    [InlineData("invalid_acceess_token", ShopeeErrorCategory.Authentication)]
    [InlineData("error_sign", ShopeeErrorCategory.Signature)]
    [InlineData("error_param", ShopeeErrorCategory.Validation)]
    [InlineData("error_server", ShopeeErrorCategory.Transient)]
    [InlineData("error_system_busy", ShopeeErrorCategory.Transient)]
    [InlineData("unknown_code", ShopeeErrorCategory.Unknown)]
    public void Classify_MapsShopeeErrorCode(string error, ShopeeErrorCategory expected)
    {
        ShopeeErrorClassifier.Classify(error).Should().Be(expected);
    }
}
