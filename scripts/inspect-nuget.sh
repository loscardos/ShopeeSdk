#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACTS="${1:-$ROOT/artifacts/packages}"
VERSION="${SHOPEE_SDK_PACKAGE_VERSION:-0.1.1-preview}"
TMP="$ROOT/.tmp/nuget-inspect"

packages=(
  "Loscardos.ShopeeSdk.Abstractions"
  "Loscardos.ShopeeSdk.Core"
  "Loscardos.ShopeeSdk.Extensions.DependencyInjection"
  "Loscardos.ShopeeSdk.Generated"
  "Loscardos.ShopeeSdk.Storage.EntityFramework"
)

rm -rf "$TMP"
mkdir -p "$TMP"

for package in "${packages[@]}"; do
  nupkg="$ARTIFACTS/$package.$VERSION.nupkg"
  if [[ ! -f "$nupkg" ]]; then
    echo "Missing package: $nupkg"
    exit 1
  fi

  package_dir="$TMP/$package"
  mkdir -p "$package_dir"
  unzip -qq "$nupkg" -d "$package_dir"

  assembly="$package"
  nuspec="$package_dir/$package.nuspec"

  [[ -f "$package_dir/README.md" ]] || { echo "$package is missing README.md"; exit 1; }
  [[ -f "$package_dir/lib/net8.0/$assembly.dll" ]] || { echo "$package is missing lib/net8.0/$assembly.dll"; exit 1; }
  [[ -f "$package_dir/lib/net8.0/$assembly.xml" ]] || { echo "$package is missing lib/net8.0/$assembly.xml"; exit 1; }
  [[ -f "$nuspec" ]] || { echo "$package is missing nuspec"; exit 1; }

  grep -q "<id>$package</id>" "$nuspec" || { echo "$package nuspec has wrong id"; exit 1; }
  grep -q "<version>$VERSION</version>" "$nuspec" || { echo "$package nuspec has wrong version"; exit 1; }
  grep -q "<license type=\"expression\">MIT</license>" "$nuspec" || { echo "$package nuspec is missing MIT license"; exit 1; }
  grep -q "<readme>README.md</readme>" "$nuspec" || { echo "$package nuspec is missing package readme"; exit 1; }

  if find "$package_dir" | grep -Eq '(^|/)(\.token)(/|$)'; then
    echo "$package contains ignored local-only assets"
    exit 1
  fi
done

echo "NuGet packages inspected."
