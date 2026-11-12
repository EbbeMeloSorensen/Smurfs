#!/usr/bin/env bash
# Build the documentation locally without PowerShell or a rendering service.
# Usage: bash docs/build-docs.sh [path/to/plantuml.jar]
set -euo pipefail

fail() { printf 'Error: %s\n' "$*" >&2; exit 1; }

if (( $# > 1 )); then
    fail 'Usage: bash docs/build-docs.sh [path/to/plantuml.jar]'
fi

docs_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
output_root="$docs_root/_generated/diagrams"

for command_name in java dot docfx find mktemp python3; do
    command -v "$command_name" >/dev/null 2>&1 || fail "Required command not found: $command_name"
done
[[ -f "$docs_root/docfx.json" ]] || fail "Missing $docs_root/docfx.json"

# Resolve a caller-relative JAR before changing to the documentation directory.
plantuml_jar="${1-${PLANTUML_JAR:-}}"
if [[ -n "$plantuml_jar" ]]; then
    [[ -f "$plantuml_jar" ]] || fail "PlantUML JAR not found: $plantuml_jar"
    plantuml_jar="$(cd -- "$(dirname -- "$plantuml_jar")" && pwd -P)/$(basename -- "$plantuml_jar")"
    renderer=(java -Djava.awt.headless=true -jar "$plantuml_jar")
else
    command -v plantuml >/dev/null 2>&1 || fail 'Install plantuml, set PLANTUML_JAR, or pass a local PlantUML JAR path.'
    renderer=(plantuml)
fi

cd -- "$docs_root"
python3 prepare-site-pages.py || fail 'Site page preparation failed.'
python3 generate-project-pages.py || fail 'Project documentation generation failed.'
python3 validate-traceability.py || fail 'Traceability validation failed.'
# A temporary list propagates discovery failures and supports spaces in filenames.
model_list="$(mktemp)"
trap 'rm -f -- "$model_list"' EXIT
find . -type d \( -name _site -o -name _generated -o -name api \) -prune -o \
    -type f -name '*.puml' -print0 > "$model_list"
[[ -s "$model_list" ]] || fail 'No PlantUML sources found.'

while IFS= read -r -d '' model; do
    relative_path="${model#./}"
    destination="$output_root/$(dirname -- "$relative_path")"
    expected_svg="$destination/$(basename -- "${relative_path%.puml}").svg"
    mkdir -p -- "$destination"

    printf 'Validating %s\n' "$relative_path"
    "${renderer[@]}" -charset UTF-8 -checkonly "$model" || fail "PlantUML validation failed: $relative_path"
    # Remove only this generated output so an old SVG cannot mask missing output.
    rm -f -- "$expected_svg"
    "${renderer[@]}" -charset UTF-8 -tsvg -failfast2 -o "$destination" "$model" || fail "PlantUML rendering failed: $relative_path"
    [[ -f "$expected_svg" ]] || fail "Missing SVG for $relative_path"
done < "$model_list"

docfx metadata docfx.json || fail 'DocFX metadata failed.'
docfx build docfx.json --warningsAsErrors || fail 'DocFX build failed.'
printf 'Documentation built: %s/_site/index.html\n' "$docs_root"
