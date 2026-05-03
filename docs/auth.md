# Authentication

Shopee Open API uses an authorization code flow. The SDK provides `IAuthApi` for building the authorization URL, exchanging the code, refreshing tokens, and storing token results.

## Build the Authorization URL

```csharp
using ShopeeSdk.Abstractions.Managers;

var redirectUri = new Uri("https://example.com/shopee/callback");
var url = authApi.BuildAuthorizationUrl(redirectUri);
```

Send the seller to `url`. After authorization, Shopee redirects back to your `redirectUri` with a `code` and shop or merchant identifier.

## Exchange Code

```csharp
var token = await authApi.ExchangeCodeAsync(
    code,
    shopId,
    merchantId: null,
    cancellationToken);
```

`ExchangeCodeAsync` stores the token through the configured `IShopeeTokenStore`.

## Refresh Token

```csharp
var refreshed = await authApi.RefreshAsync(
    shopId,
    merchantId: null,
    cancellationToken);
```

The SDK also includes `ShopeeTokenService`, which is used by authenticated managers to refresh expired tokens with a lock per shop or merchant. This prevents parallel requests from rotating the same token multiple times.

## Production Notes

- Store `PartnerKey`, access tokens, and refresh tokens in secure infrastructure.
- Do not place shop access tokens in application settings for production.
- Persist `shop_id` or `merchant_id` with the tenant record that owns the authorization.
- Treat authorization codes as one-time credentials.
- Refresh tokens before expiry. The default refresh skew is 5 minutes.

## Local Sandbox Environment

For local sample-console testing, use ignored `.token` files:

```bash
SHOPEE_PARTNER_ID=
SHOPEE_PARTNER_KEY=
SHOPEE_REGION=TestGlobal
SHOPEE_REDIRECT_URL=
SHOPEE_SHOP_ID=
SHOPEE_AUTH_CODE=
SHOPEE_ACCESS_TOKEN=
SHOPEE_REFRESH_TOKEN=
```

The sample console prints token lengths and timestamps instead of token values.
