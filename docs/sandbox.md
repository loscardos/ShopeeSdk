# Sandbox Validation

The sample console supports sandbox auth and read-only checks without committing credentials.

## Environment File

Create an ignored local file such as `.token/shopee-sandbox.env`:

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

Load it in your shell before running sample commands:

```bash
set -a
. .token/shopee-sandbox.env
set +a
```

## Commands

Generate authorization URL:

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- auth-url
```

Exchange a sandbox authorization code:

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- exchange-code
```

Refresh token and update the env file:

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- refresh-token --update-env .token/shopee-sandbox.env
```

Run a read-only product and shop check:

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- sandbox-check
```

Write an offline endpoint certification matrix from generated managers:

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- certify-matrix
```

Run broader read-only canary certification and write `.tmp/shopee-readonly-certification.md`:

```bash
dotnet run --project samples/ShopeeSdk.SampleConsole -- certify-readonly
```

`certify-readonly` calls representative read-only product, shop, and order endpoints. Order detail, payment escrow, and logistics tracking are marked `SkipNoData` unless `SHOPEE_CANARY_ORDER_SN` or `SHOPEE_SANDBOX_ORDER_SN` is configured. `SHOPEE_CANARY_PACKAGE_NUMBER` or `SHOPEE_SANDBOX_PACKAGE_NUMBER` is optional for logistics tracking.

## Integration Tests

Sandbox integration tests are skipped by default. Enable them explicitly:

```bash
SHOPEE_RUN_SANDBOX=true dotnet test tests/ShopeeSdk.IntegrationTests/ShopeeSdk.IntegrationTests.csproj
```

Order-specific checks also require:

```bash
SHOPEE_SANDBOX_ORDER_SN=
SHOPEE_SANDBOX_PACKAGE_NUMBER=
```

`SHOPEE_SANDBOX_PACKAGE_NUMBER` is optional for tracking checks.
