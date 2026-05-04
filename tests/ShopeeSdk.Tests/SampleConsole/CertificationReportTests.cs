using FluentAssertions;
using ShopeeSdk.SampleConsole;

namespace ShopeeSdk.Tests.SampleConsole;

public sealed class CertificationReportTests
{
    [Fact]
    public void ToMarkdown_WritesStatusSummaryAndEscapesTableValues()
    {
        var markdown = CertificationReport.ToMarkdown([
            new CertificationResult("Product", "GET /product/get_category", CertificationStatus.Pass, "categories=3"),
            new CertificationResult("Order", "GET /order/get_order_detail", CertificationStatus.SkipNoData, "needs order|sn")
        ]);

        markdown.Should().Contain("# Shopee SDK Readonly Certification");
        markdown.Should().Contain("| Pass | 1 |");
        markdown.Should().Contain("| SkipNoData | 1 |");
        markdown.Should().Contain("| Order | GET /order/get_order_detail | SkipNoData | needs order\\|sn |");
    }
}
