#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACTS="$ROOT/artifacts"
TMP="$ROOT/.tmp/local-nuget-consumer"

rm -rf "$ARTIFACTS" "$TMP"
dotnet pack "$ROOT/ShopeeSdk.sln" --output "$ARTIFACTS"

mkdir -p "$TMP"
dotnet new console -n LocalNuGetConsumer -o "$TMP"
dotnet add "$TMP/LocalNuGetConsumer.csproj" package Loscardos.ShopeeSdk.Core --version 0.1.0-preview --source "$ARTIFACTS"
dotnet add "$TMP/LocalNuGetConsumer.csproj" package Loscardos.ShopeeSdk.Extensions.DependencyInjection --version 0.1.0-preview --source "$ARTIFACTS"
dotnet add "$TMP/LocalNuGetConsumer.csproj" package Loscardos.ShopeeSdk.Storage.EntityFramework --version 0.1.0-preview --source "$ARTIFACTS"
dotnet add "$TMP/LocalNuGetConsumer.csproj" package Microsoft.Extensions.DependencyInjection --version 10.0.7

cat > "$TMP/Program.cs" <<'CS'
using Microsoft.Extensions.DependencyInjection;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddShopeeSdk(options =>
{
    options.PartnerId = 1;
    options.PartnerKey = "key";
    options.Region = ShopeeRegion.TestGlobal;
});

using var provider = services.BuildServiceProvider();
Console.WriteLine(provider.GetRequiredService<ShopeeSdk.Abstractions.Http.IShopeeClient>().GetType().Name);
CS

dotnet run --project "$TMP/LocalNuGetConsumer.csproj"
