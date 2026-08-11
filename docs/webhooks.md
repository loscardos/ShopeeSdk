# Webhooks

The SDK provides Shopee-specific webhook primitives:

- verify callback signatures with `IShopeeWebhookVerifier`
- parse raw Shopee callback envelopes
- validate callback timestamps
- compute deterministic idempotency keys
- parse known Shopee push payloads into typed DTOs

The SDK intentionally does not own broker, storage, or worker infrastructure. A production receiver should persist or enqueue the verified raw envelope, return `200 OK` quickly, and process the event in a separate worker.

Recommended receiver flow:

```text
Shopee
  -> ASP.NET webhook endpoint
  -> verify signature
  -> optionally validate timestamp tolerance
  -> compute idempotency key
  -> save or enqueue raw verified envelope
  -> return 200
  -> worker parses typed payload and runs business processing
```

The captured Shopee push mechanism reference is stored at `docs/webhooks/shopee-push-mechanisms.yaml`.

## Options

Webhook verification options can be configured through `AddShopeeSdk`:

```csharp
builder.Services.AddShopeeSdk(
    options =>
    {
        options.PartnerId = partnerId;
        options.PartnerKey = partnerKey;
        options.PushPartnerKey = pushPartnerKey;
        options.PushCallbackUrl = "https://example.com/webhooks/shopee";
    },
    webhooks =>
    {
        webhooks.SignatureHeaderName = "authorization";
        webhooks.RequireFreshTimestamp = false;
        webhooks.TimestampTolerance = TimeSpan.FromMinutes(10);
    });
```

The default signature header name is `authorization`. Set `PushCallbackUrl` to the exact public callback URL registered with Shopee; the push signature base is `callbackUrl + "|" + rawBody`. The callback URL is case-sensitive and the raw body must not be parsed and re-serialized before verification. The captured Shopee push mechanism specs describe callback payloads and retry behavior, but do not state a signature header name, so keep the header configurable and verify it against a real callback capture for the target Shopee app/region.

Timestamp freshness rejection is disabled by default. Shopee can retry the same signed message hours later and the console Test Push can carry a fixed sample timestamp. Use signature verification plus durable idempotency for replay safety. Enable `RequireFreshTimestamp` only when the target integration has a separately verified freshness contract and a tolerance that covers provider retries.

To verify a captured callback end-to-end, run the webhook canary with the exact raw request body and signature value received from Shopee:

```bash
SHOPEE_RUN_SANDBOX=true \
SHOPEE_PARTNER_ID=... \
SHOPEE_PARTNER_KEY=... \
SHOPEE_WEBHOOK_PAYLOAD_PATH=/path/to/raw-callback.json \
SHOPEE_WEBHOOK_SIGNATURE=... \
dotnet test tests/ShopeeSdk.IntegrationTests/ShopeeSdk.IntegrationTests.csproj --filter "FullyQualifiedName~ShopeeWebhookCallbackCanaryTests"
```

If the callback is old enough to fail timestamp tolerance, also set `SHOPEE_WEBHOOK_RECEIVED_AT_UNIX` to the Unix timestamp when the callback was received.

## Responsibilities

SDK responsibilities:

- verify Shopee signature
- optionally validate callback timestamp when explicitly configured
- compute idempotency key
- parse known Shopee push payloads
- expose raw envelope interfaces

Application responsibilities:

- implement `IShopeeWebhookQueue`
- implement `IShopeeWebhookRawPayloadSink` when raw payload archiving is required
- implement idempotency storage
- run worker consumers
- normalize Shopee/TikTok/Tokopedia events into internal domain events

## Worker Flow

```text
Raw verified envelope
  -> broker message
  -> worker receives message
  -> idempotency store checks key
  -> parse typed Shopee payload
  -> map to application event
  -> commit offset or ack message
```
