using FluentAssertions;
using Loscardos.ShopeeSdk.SampleConsole;

namespace Loscardos.ShopeeSdk.Tests.SampleConsole;

public sealed class SandboxEnvFileTests
{
    [Fact]
    public void UpdateTokens_ReplacesExistingTokenValuesAndPreservesOtherLines()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.env");
        File.WriteAllLines(path, [
            "SHOPEE_PARTNER_ID=123",
            "SHOPEE_ACCESS_TOKEN=old-access",
            "# comment",
            "SHOPEE_REFRESH_TOKEN=old-refresh",
            "SHOPEE_SHOP_ID=456"
        ]);

        try
        {
            SandboxEnvFile.UpdateTokens(path, "new-access", "new-refresh");

            File.ReadAllLines(path).Should().Equal([
                "SHOPEE_PARTNER_ID=123",
                "SHOPEE_ACCESS_TOKEN=new-access",
                "# comment",
                "SHOPEE_REFRESH_TOKEN=new-refresh",
                "SHOPEE_SHOP_ID=456"
            ]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UpdateTokens_AppendsMissingTokenValues()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.env");
        File.WriteAllLines(path, [
            "SHOPEE_PARTNER_ID=123"
        ]);

        try
        {
            SandboxEnvFile.UpdateTokens(path, "new-access", "new-refresh");

            File.ReadAllLines(path).Should().Equal([
                "SHOPEE_PARTNER_ID=123",
                "SHOPEE_ACCESS_TOKEN=new-access",
                "SHOPEE_REFRESH_TOKEN=new-refresh"
            ]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
