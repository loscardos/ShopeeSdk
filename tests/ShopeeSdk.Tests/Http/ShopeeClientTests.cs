using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Errors;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Http;

public sealed class ShopeeClientTests
{
    [Fact]
    public async Task SendAsync_AddsCommonParamsAndReturnsEnvelope()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://partner.test-stable.shopeemobile.com/api/v2/product/get_item_list*")
            .WithQueryString("partner_id", "123456")
            .WithQueryString("timestamp", "1610000000")
            .WithQueryString("offset", "0")
            .WithQueryString("page_size", "10")
            .Respond("application/json", """
            {
              "request_id": "req-1",
              "error": "",
              "message": "",
              "response": { "total_count": 0, "item": [], "has_next_page": false }
            }
            """);

        var client = CreateClient(mockHttp);

        var response = await client.SendAsync<object>(
            new ShopeeRequest(
                ShopeeRequestMethod.Get,
                "/product/get_item_list",
                new Dictionary<string, object?> { ["offset"] = 0, ["page_size"] = 10 },
                null,
                null),
            CancellationToken.None);

        response.RequestId.Should().Be("req-1");
        response.Error.Should().BeEmpty();
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task SendAsync_ThrowsTypedApiExceptionForShopeeError()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(HttpMethod.Get, "https://partner.test-stable.shopeemobile.com/api/v2/product/get_item_list*")
            .Respond(HttpStatusCode.OK, "application/json", """
            {
              "request_id": "req-2",
              "error": "error_sign",
              "message": "Wrong sign."
            }
            """);

        var client = CreateClient(mockHttp);

        var act = async () => await client.SendAsync<object>(
            new ShopeeRequest(ShopeeRequestMethod.Get, "/product/get_item_list", new Dictionary<string, object?>(), null, null),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ShopeeApiException>();
        exception.Which.Error.Should().Be("error_sign");
        exception.Which.RequestId.Should().Be("req-2");
    }

    private static ShopeeClient CreateClient(MockHttpMessageHandler mockHttp)
    {
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://partner.test-stable.shopeemobile.com/api/v2/");
        return new ShopeeClient(
            httpClient,
            Options.Create(new ShopeeOptions
            {
                PartnerId = 123456,
                PartnerKey = "test_partner_key",
                Region = ShopeeRegion.TestGlobal
            }),
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1610000000)));
    }
}
