#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

before="$(git status --short)"

dotnet build ShopeeSdk.sln --no-restore
dotnet test tests/ShopeeSdk.Tests/ShopeeSdk.Tests.csproj --no-build --filter "ErpEndpointCoverageTests|PublicApiPolishTests|GeneratedAuthScopeTests|GeneratedOutputDeterminismTests|GeneratedManagerOutputTests|SchemaReaderTests|SchemaTypeCoverageTests"

after="$(git status --short)"

if [[ "$before" != "$after" ]]; then
  echo "Generated output is not deterministic or generated files are stale."
  echo "Before:"
  printf '%s\n' "$before"
  echo "After:"
  printf '%s\n' "$after"
  exit 1
fi

echo "Generated SDK surface verified."
