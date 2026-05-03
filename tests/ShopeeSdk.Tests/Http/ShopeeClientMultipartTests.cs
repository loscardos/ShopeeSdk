using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Core.RateLimiting;
using ShopeeSdk.Tests.TestDoubles;

namespace ShopeeSdk.Tests.Http;

public sealed class ShopeeClientMultipartTests
{
    [Fact]
    public async Task SendAsync_SendsMultipartFilePart()
    {
        using var handler = new CapturingHandler("""{"request_id":"req-1","error":"","message":"","response":{"image_list":[]}}""");
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://partner.test/api/v2/") };
        var client = new ShopeeClient(
            httpClient,
            Options.Create(new ShopeeOptions { PartnerId = 123, PartnerKey = "secret", BaseUrl = new Uri("https://partner.test/api/v2/") }),
            new FixedShopeeClock(DateTimeOffset.FromUnixTimeSeconds(1700000000)),
            new NoopShopeeRateLimiter(),
            NullLogger<ShopeeClient>.Instance);

        await client.SendAsync<object>(
            new ShopeeRequest(
                ShopeeRequestMethod.Post,
                "/media/upload_image",
                new Dictionary<string, object?>(),
                new Dictionary<string, object?>
                {
                    ["images"] = new ShopeeFilePart("image.png", "image/png", Encoding.UTF8.GetBytes("png"))
                },
                null,
                ShopeeRequestContentKind.Multipart),
            CancellationToken.None);

        handler.Request!.Content.Should().BeOfType<MultipartFormDataContent>();
        handler.Body.Should().Contain("name=images");
        handler.Body.Should().Contain("filename=image.png");
    }

    private sealed class CapturingHandler(string payload) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        }
    }
}
