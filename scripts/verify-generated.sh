#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

before="$(git status --short)"

dotnet run --project src/ShopeeSdk.Generator -- generate reference/schemas docs
dotnet build ShopeeSdk.sln --no-restore

after="$(git status --short)"

if [[ "$before" != "$after" ]]; then
  echo "Generated output is not deterministic or generated files are stale."
  echo "Before:"
  printf '%s\n' "$before"
  echo "After:"
  printf '%s\n' "$after"
  exit 1
fi

grep -q "Total schema endpoints: 425" docs/endpoint-coverage.md
echo "Generated output verified."
