using FluentAssertions;
using Loscardos.ShopeeSdk.Generator;

namespace Loscardos.ShopeeSdk.Tests.Generator;

public sealed class SchemaReaderTests
{
    [Fact]
    public void ReadDirectory_ReadsSchemaMetadata()
    {
        var reader = new SchemaReader();

        var endpoints = reader.ReadDirectory(SchemaFixturePaths.FindSchemas());

        endpoints.Should().Contain(endpoint =>
            endpoint.ApiName == "v2.product.get_category"
            && endpoint.ModuleName == "Product"
            && endpoint.ModuleKey == "product"
            && endpoint.Path == "/product/get_category"
            && endpoint.HttpMethod == ShopeeSchemaHttpMethod.Get
            && endpoint.AuthScope == ShopeeSchemaAuthScope.Shop
            && endpoint.RequestParameters.Any(parameter => parameter.Name == "language" && parameter.Type == "string" && !parameter.Required));

        endpoints.Should().Contain(endpoint =>
            endpoint.ApiName == "v2.media.upload_image"
            && endpoint.ModuleName == "Media"
            && endpoint.ModuleKey == "media"
            && endpoint.Path == "/media/upload_image"
            && endpoint.HttpMethod == ShopeeSchemaHttpMethod.Post
            && endpoint.AuthScope == ShopeeSchemaAuthScope.Public
            && endpoint.RequestContentKind == ShopeeSchemaRequestContentKind.Multipart
            && endpoint.RequestParameters.Any(parameter => parameter.Name == "images" && parameter.Type == "file" && parameter.Required));
    }
}
