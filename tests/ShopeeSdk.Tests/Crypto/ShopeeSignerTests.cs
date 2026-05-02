using FluentAssertions;
using ShopeeSdk.Core.Crypto;

namespace ShopeeSdk.Tests.Crypto;

public sealed class ShopeeSignerTests
{
    [Fact]
    public void Sign_UsesHmacSha256OverConcatenatedParts()
    {
        var signature = ShopeeSigner.Sign(
            "test_partner_key",
            ["123456", "/api/v2/shop/auth_partner", "1610000000"]);

        signature.Should().Be("50eba3a5c39a5da58caa3a5057a3c879e70a4e4ec764fef6423ba7ae6750df41");
    }

    [Fact]
    public void Sign_ChangesWhenAnyPartChanges()
    {
        var first = ShopeeSigner.Sign("test_partner_key", ["123456", "/api/v2/shop/auth_partner", "1610000000"]);
        var second = ShopeeSigner.Sign("test_partner_key", ["123456", "/api/v2/shop/auth_partner", "1610000001"]);

        second.Should().NotBe(first);
    }
}
