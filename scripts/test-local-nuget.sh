#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACTS="$ROOT/artifacts/packages"
TMP="$ROOT/.tmp/local-nuget-consumer"

rm -rf "$TMP"
mkdir -p "$ARTIFACTS"
find "$ARTIFACTS" -maxdepth 1 -type f -name '*.nupkg' -delete
rm -rf "$HOME/.nuget/packages/loscardos.shopeesdk."*
"$ROOT/scripts/pack-local.sh" "$ARTIFACTS"
"$ROOT/scripts/inspect-nuget.sh" "$ARTIFACTS"

mkdir -p "$TMP"
dotnet new console -n LocalNuGetConsumer -o "$TMP"

cat > "$TMP/NuGet.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="shopeesdk-local" value="$ARTIFACTS" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF

dotnet add "$TMP/LocalNuGetConsumer.csproj" package Loscardos.ShopeeSdk.Core --version 0.1.0-preview --no-restore
dotnet add "$TMP/LocalNuGetConsumer.csproj" package Loscardos.ShopeeSdk.Extensions.DependencyInjection --version 0.1.0-preview --no-restore
dotnet add "$TMP/LocalNuGetConsumer.csproj" package Loscardos.ShopeeSdk.Storage.EntityFramework --version 0.1.0-preview --no-restore
dotnet add "$TMP/LocalNuGetConsumer.csproj" package Microsoft.Extensions.DependencyInjection --version 10.0.7 --no-restore

cat > "$TMP/Program.cs" <<'CS'
using Microsoft.Extensions.DependencyInjection;
using ShopeeSdk.Abstractions.Auth;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Abstractions.Http;
using ShopeeSdk.Abstractions.Managers.Generated;
using ShopeeSdk.Extensions.DependencyInjection;
using ShopeeSdk.Generated.Product;

var services = new ServiceCollection();
services.AddSingleton<IShopeeTokenStore, SmokeTokenStore>();
services.AddShopeeSdk(options =>
{
    options.PartnerId = 1;
    options.PartnerKey = "key";
    options.Region = ShopeeRegion.TestGlobal;
});

using var provider = services.BuildServiceProvider();
Console.WriteLine(provider.GetRequiredService<IShopeeClient>().GetType().Name);
Console.WriteLine(provider.GetRequiredService<IProductApi>().GetType().FullName);
Console.WriteLine(typeof(ProductGetCategoryRequest).Name);

public sealed class SmokeTokenStore : IShopeeTokenStore
{
    public Task<ShopeeToken?> GetAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        return Task.FromResult<ShopeeToken?>(new ShopeeToken(
            shopId,
            merchantId,
            "access",
            "refresh",
            DateTimeOffset.UtcNow.AddHours(1),
            "smoke"));
    }

    public Task StoreAsync(ShopeeToken token, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task ClearAsync(long? shopId, long? merchantId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
CS

dotnet restore "$TMP/LocalNuGetConsumer.csproj" --configfile "$TMP/NuGet.config"
dotnet run --project "$TMP/LocalNuGetConsumer.csproj" --no-restore
