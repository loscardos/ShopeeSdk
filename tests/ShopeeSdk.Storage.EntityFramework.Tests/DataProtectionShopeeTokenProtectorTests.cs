using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;

namespace ShopeeSdk.Storage.EntityFramework.Tests;

public sealed class DataProtectionShopeeTokenProtectorTests
{
    [Fact]
    public void Protect_RoundTripsWithoutStoringPlainText()
    {
        var provider = DataProtectionProvider.Create("ShopeeSdk.Tests");
        var protector = new DataProtectionShopeeTokenProtector(provider);

        var protectedValue = protector.Protect("secret-token");

        protectedValue.Should().NotBe("secret-token");
        protector.Unprotect(protectedValue).Should().Be("secret-token");
    }
}
