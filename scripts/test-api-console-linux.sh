#!/usr/bin/env bash
# Run on Linux with the .NET 10 SDK; no running API or desktop keyring is required.
set -euo pipefail

if [[ "$(uname -s)" != "Linux" ]]; then
    echo "Run this verification on Linux so native credential permission tests execute." >&2
    exit 1
fi

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd -- "$script_dir/.." && pwd)"
dotnet test "$repo_root/tests/Smurf.Persistence.EFCore.AppData.UnitTest" \
    --filter 'FullyQualifiedName~LinuxTokenStoreTests|FullyQualifiedName~ApiConsoleTests|FullyQualifiedName~ApiClientTests'
