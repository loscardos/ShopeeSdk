using FluentAssertions;
using Loscardos.ShopeeSdk.Abstractions.Errors;
using Loscardos.ShopeeSdk.Core.Validation;

namespace Loscardos.ShopeeSdk.Tests.Validation;

public sealed class ShopeeRequestValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void EnsurePageSize_ThrowsOutsideRange(int pageSize)
    {
        var act = () => ShopeeRequestValidator.EnsurePageSize(pageSize, 1, 100, "page_size");

        act.Should().Throw<ShopeeSdkException>().WithMessage("*page_size*");
    }

    [Fact]
    public void EnsureTimeRange_ThrowsWhenRangeExceedsLimit()
    {
        var act = () => ShopeeRequestValidator.EnsureTimeRange(0, 16 * 24 * 60 * 60, TimeSpan.FromDays(15), "order time range");

        act.Should().Throw<ShopeeSdkException>().WithMessage("*order time range*");
    }

    [Fact]
    public void EnsureRequiredString_ThrowsForBlankValue()
    {
        var act = () => ShopeeRequestValidator.EnsureRequiredString(" ", "order_sn");

        act.Should().Throw<ShopeeSdkException>().WithMessage("*order_sn*");
    }
}
