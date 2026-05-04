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
  -> validate timestamp tolerance
  -> compute idempotency key
  -> save or enqueue raw verified envelope
  -> return 200
  -> worker parses typed payload and runs business processing
```

The captured Shopee push mechanism reference is stored at `docs/webhooks/shopee-push-mechanisms.yaml`.

## Responsibilities

SDK responsibilities:

- verify Shopee signature
- validate callback timestamp
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
