# Packaging and Verification

The repo ships NuGet packages under `Loscardos.ShopeeSdk.*`.

Shipping assemblies and C# namespaces use the same `Loscardos.ShopeeSdk.*`
identity. Package versions are immutable after publication.

## Local Verification

Run the same core checks used by CI:

```bash
dotnet restore ShopeeSdk.sln
dotnet build ShopeeSdk.sln --no-restore
dotnet test ShopeeSdk.sln --no-build
bash scripts/verify-generated.sh
```

## Package Build

```bash
bash scripts/pack-local.sh
bash scripts/inspect-nuget.sh
bash scripts/test-local-nuget.sh
```

Local packages are written to `artifacts/packages`, which is also registered as `shopeesdk-local` in `NuGet.Config`.

`inspect-nuget.sh` checks package contents for common packaging mistakes. `test-local-nuget.sh` creates a temporary consumer project and installs the local packages from `artifacts/packages`.

## Package Metadata

Shared package metadata is configured in `Directory.Build.props`:

- Version: `0.1.0-preview`
- Authors: `Loscardos`
- License expression: `MIT`
- Tags: `shopee;openapi;sdk;aspnet;omnichannel;ecommerce`
- Deterministic builds enabled
- XML docs generated

## CI

`.github/workflows/ci.yml` runs:

- restore
- build
- tests
- pack
- local NuGet consumer validation
- generated SDK surface verification
- offline endpoint certification matrix generation

Sandbox tests stay gated and are not run unless sandbox environment variables are explicitly configured.

Development and staging pushes run validation. A production push does not run
the test suite again; it packs the version already declared in
`Directory.Build.props` and publishes it to the private
`https://nuget.pkg.github.com/loscardos/index.json` feed. Publishing the same
version twice fails, so every changed release must increment `VersionPrefix` or
`VersionSuffix` before promotion.
