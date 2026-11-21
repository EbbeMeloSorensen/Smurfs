#!/usr/bin/env bash
# Usage: bash scripts/add-appdata-migrations.sh DescribeChange
set -euo pipefail

if [[ $# -ne 1 || -z "$1" ]]; then
    printf 'Usage: bash scripts/add-appdata-migrations.sh DescribeChange\n' >&2
    exit 2
fi

DescribeChange="$1"
repo_root="$(cd -- "$(dirname -- "$0")/.." && pwd -P)"
project="$repo_root/src/Smurfs.Persistence.EFCore.AppData/Smurfs.Persistence.EFCore.AppData.csproj"

command -v dotnet >/dev/null 2>&1 || { printf 'Install the .NET 10 SDK and add dotnet to PATH.\n' >&2; exit 1; }
cd -- "$repo_root"
dotnet ef --version

for provider in Postgres Sqlite; do
    printf 'Adding %s migration: %s\n' "$provider" "$DescribeChange"
    if ! dotnet ef migrations add "$DescribeChange" \
        --project "$project" \
        --startup-project "$project" \
        --context "Smurfs.Persistence.EFCore.AppData.$provider"DataContext \
        --output-dir "Migrations/$provider"; then
        printf '%s migration failed. Inspect git diff before retrying; an earlier provider may already have been generated.\n' "$provider" >&2
        exit 1
    fi
done
