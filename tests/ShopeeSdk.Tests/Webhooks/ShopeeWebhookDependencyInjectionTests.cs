using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Loscardos.ShopeeSdk.Abstractions.Webhooks;
using Loscardos.ShopeeSdk.Core.Webhooks;
using Loscardos.ShopeeSdk.Extensions.DependencyInjection;

namespace Loscardos.ShopeeSdk.Tests.Webhooks;

public sealed class ShopeeWebhookDependencyInjectionTests
{
    [Fact]
    public void AddShopeeSdk_RegistersWebhookParserAndIdempotencyFactory()
    {
        var services = new ServiceCollection();
        services.AddShopeeSdk(options =>
        {
            options.PartnerId = 1;
            options.PartnerKey = "partner-key";
        });

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IShopeeWebhookParser>().Should().NotBeNull();
        provider.GetRequiredService<IShopeeWebhookIdempotencyKeyFactory>().Should().NotBeNull();
    }

    [Fact]
    public void AddShopeeSdk_AppliesWebhookOptions()
    {
        var services = new ServiceCollection();
        services.AddShopeeSdk(
            options =>
            {
                options.PartnerId = 1;
                options.PartnerKey = "partner-key";
            },
            webhooks =>
            {
                webhooks.TimestampTolerance = TimeSpan.FromMinutes(2);
                webhooks.SignatureHeaderName = "x-shopee-signature";
            });

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<ShopeeWebhookOptions>();
        options.TimestampTolerance.Should().Be(TimeSpan.FromMinutes(2));
        options.SignatureHeaderName.Should().Be("x-shopee-signature");
    }
}
