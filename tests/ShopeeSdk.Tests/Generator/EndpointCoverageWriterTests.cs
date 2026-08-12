using FluentAssertions;
using Loscardos.ShopeeSdk.Generator;

namespace Loscardos.ShopeeSdk.Tests.Generator;

public sealed class EndpointCoverageWriterTests
{
    [Fact]
    public void WriteMarkdown_IncludesCountsAndDomainRows()
    {
        var endpoints = new[]
        {
            new SchemaEndpoint("v2.product.get_category", "Product", "product", "/product/get_category", ShopeeSchemaHttpMethod.Get, ShopeeSchemaAuthScope.Shop, ShopeeSchemaRequestContentKind.Query, [], []),
            new SchemaEndpoint("v2.media.upload_image", "Media", "media", "/media/upload_image", ShopeeSchemaHttpMethod.Post, ShopeeSchemaAuthScope.Public, ShopeeSchemaRequestContentKind.Multipart, [], [])
        };

        var markdown = new EndpointCoverageWriter().WriteMarkdown(endpoints);

        markdown.Should().Contain("# Endpoint Coverage");
        markdown.Should().Contain("| Product | 1 |");
        markdown.Should().Contain("| Media | 1 |");
        markdown.Should().Contain("Total schema endpoints: 2");
    }
}
