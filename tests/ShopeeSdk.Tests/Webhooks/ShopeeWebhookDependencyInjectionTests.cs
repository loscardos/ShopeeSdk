using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ShopeeSdk.Abstractions.Webhooks;
using ShopeeSdk.Extensions.DependencyInjection;

namespace ShopeeSdk.Tests.Webhooks;

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
}
