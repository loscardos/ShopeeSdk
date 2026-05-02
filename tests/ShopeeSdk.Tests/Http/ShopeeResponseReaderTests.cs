using FluentAssertions;
using ShopeeSdk.Core.Http;

namespace ShopeeSdk.Tests.Http;

public sealed class ShopeeResponseReaderTests
{
    [Fact]
    public void ReadJsonEnvelope_HandlesResponseWrappedPayload()
    {
        var envelope = ShopeeResponseReader.ReadJsonEnvelope<TestPayload>("""
        { "request_id": "req-1", "error": "", "message": "", "response": { "value": "ok" } }
        """);

        envelope.RequestId.Should().Be("req-1");
        envelope.Response!.Value.Should().Be("ok");
    }

    [Fact]
    public void ReadJsonEnvelope_HandlesTopLevelAuthPayload()
    {
        var envelope = ShopeeResponseReader.ReadJsonEnvelope<TestPayload>("""
        { "request_id": "req-2", "error": "", "message": "", "value": "ok" }
        """);

        envelope.RequestId.Should().Be("req-2");
        envelope.Response!.Value.Should().Be("ok");
    }

    [Fact]
    public void ReadJsonEnvelope_ThrowsSdkExceptionForEmptyPayload()
    {
        var act = () => ShopeeResponseReader.ReadJsonEnvelope<TestPayload>("");
        act.Should().Throw<Exception>().WithMessage("*empty*");
    }

    private sealed record TestPayload(string Value);
}
