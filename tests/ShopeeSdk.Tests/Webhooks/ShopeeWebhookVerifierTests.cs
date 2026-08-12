using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Loscardos.ShopeeSdk.Abstractions.Configuration;
using Loscardos.ShopeeSdk.Core.Webhooks;

namespace Loscardos.ShopeeSdk.Tests.Webhooks;

public sealed class ShopeeWebhookVerifierTests
{
    [Fact]
    public void Verify_UsesExactCallbackUrlAndRawBodyWhenConfigured()
    {
        const string callbackUrl = "https://omni.jez.my.id/webhooks/shopee";
        const string body = """{"shop_id":600000,"code":3,"data":{"ordersn":"ORDER-1"}}""";
        var signature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("push-partner-key"),
            Encoding.UTF8.GetBytes($"{callbackUrl}|{body}"))).ToLowerInvariant();
        var verifier = new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions
        {
            PartnerKey = "api-partner-key",
            PushPartnerKey = "push-partner-key",
            PushCallbackUrl = callbackUrl
        }));

        verifier.Verify(body, signature).Should().BeTrue();
    }

    [Fact]
    public void Verify_DoesNotAcceptBodyOnlySignatureWhenCallbackUrlIsConfigured()
    {
        const string body = """{"shop_id":600000,"code":3,"data":{"ordersn":"ORDER-1"}}""";
        var bodyOnlySignature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("push-partner-key"),
            Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        var verifier = new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions
        {
            PushPartnerKey = "push-partner-key",
            PushCallbackUrl = "https://omni.jez.my.id/webhooks/shopee"
        }));

        verifier.Verify(body, bodyOnlySignature).Should().BeFalse();
    }

    [Fact]
    public void Verify_UsesPushPartnerKeyWhenConfigured()
    {
        const string body = """{"shop_id":600000,"code":3,"data":{"ordersn":"ORDER-1"}}""";
        var signature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("push-partner-key"),
            Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        var verifier = new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions
        {
            PartnerKey = "api-partner-key",
            PushPartnerKey = "push-partner-key"
        }));

        verifier.Verify(body, signature).Should().BeTrue();
    }

    [Fact]
    public void Verify_DoesNotAcceptApiPartnerKeyWhenPushPartnerKeyIsConfigured()
    {
        const string body = """{"shop_id":600000,"code":3,"data":{"ordersn":"ORDER-1"}}""";
        var apiSignature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("api-partner-key"),
            Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        var verifier = new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions
        {
            PartnerKey = "api-partner-key",
            PushPartnerKey = "push-partner-key"
        }));

        verifier.Verify(body, apiSignature).Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsTrueForValidHmacSha256BodySignature()
    {
        const string body = """{"shop_id":600000,"code":3,"data":{"ordersn":"ORDER-1"}}""";
        var signature = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("partner-key"),
            Encoding.UTF8.GetBytes(body))).ToLowerInvariant();

        var verifier = new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions { PartnerKey = "partner-key" }));

        verifier.Verify(body, signature).Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalseForInvalidSignature()
    {
        var verifier = new ShopeeWebhookVerifier(Options.Create(new ShopeeOptions { PartnerKey = "partner-key" }));

        verifier.Verify("""{"shop_id":600000}""", "bad-signature").Should().BeFalse();
    }
}
