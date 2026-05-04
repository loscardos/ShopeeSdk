#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="${1:-$ROOT/artifacts/packages}"

mkdir -p "$OUTPUT_DIR"

dotnet pack "$ROOT/src/ShopeeSdk.Abstractions/ShopeeSdk.Abstractions.csproj" -c Release -o "$OUTPUT_DIR"
dotnet pack "$ROOT/src/ShopeeSdk.Generated/ShopeeSdk.Generated.csproj" -c Release -o "$OUTPUT_DIR"
dotnet pack "$ROOT/src/ShopeeSdk.Core/ShopeeSdk.Core.csproj" -c Release -o "$OUTPUT_DIR"
dotnet pack "$ROOT/src/ShopeeSdk.Extensions.DependencyInjection/ShopeeSdk.Extensions.DependencyInjection.csproj" -c Release -o "$OUTPUT_DIR"
dotnet pack "$ROOT/src/ShopeeSdk.Storage.EntityFramework/ShopeeSdk.Storage.EntityFramework.csproj" -c Release -o "$OUTPUT_DIR"

echo "Packages written to $OUTPUT_DIR"
