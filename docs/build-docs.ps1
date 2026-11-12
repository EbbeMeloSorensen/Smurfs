[CmdletBinding()]
param([string]$PlantUmlJar = $env:PLANTUML_JAR)
$ErrorActionPreference = 'Stop'
if (-not $PlantUmlJar -or -not (Test-Path -LiteralPath $PlantUmlJar -PathType Leaf)) {
    throw 'Set PLANTUML_JAR or pass -PlantUmlJar with a local PlantUML JAR path.'
}
$PlantUmlJar = (Resolve-Path -LiteralPath $PlantUmlJar).Path
foreach ($command in 'java', 'dot', 'docfx', 'python') {
    $null = Get-Command $command -ErrorAction Stop
}
$docsRoot = $PSScriptRoot
& python (Join-Path $docsRoot 'prepare-site-pages.py')
if ($LASTEXITCODE -ne 0) { throw 'Site page preparation failed.' }
& python (Join-Path $docsRoot 'generate-project-pages.py')
if ($LASTEXITCODE -ne 0) { throw 'Project documentation generation failed.' }
& python (Join-Path $docsRoot 'validate-traceability.py')
if ($LASTEXITCODE -ne 0) { throw 'Traceability validation failed.' }
$outputRoot = Join-Path $docsRoot '_generated/diagrams'
$models = @(Get-ChildItem -LiteralPath $docsRoot -Filter '*.puml' -Recurse -File |
    Where-Object { $_.FullName -notmatch '[\\/](_site|_generated|api)[\\/]' })
if ($models.Count -eq 0) { throw 'No PlantUML sources found.' }
foreach ($model in $models) {
    $relative = [IO.Path]::GetRelativePath($docsRoot, $model.DirectoryName)
    $destination = Join-Path $outputRoot $relative
    $null = New-Item -ItemType Directory -Path $destination -Force
    & java '-Djava.awt.headless=true' -jar $PlantUmlJar -charset UTF-8 -checkonly $model.FullName
    if ($LASTEXITCODE -ne 0) { throw "PlantUML validation failed: $($model.FullName)" }
    & java '-Djava.awt.headless=true' -jar $PlantUmlJar -charset UTF-8 -tsvg -failfast2 -o $destination $model.FullName
    if ($LASTEXITCODE -ne 0) { throw "PlantUML rendering failed: $($model.FullName)" }
    if (-not (Test-Path -LiteralPath (Join-Path $destination ($model.BaseName + '.svg')))) {
        throw "Missing SVG for $($model.FullName)"
    }
}
Push-Location $docsRoot
try {
    & docfx metadata docfx.json
    if ($LASTEXITCODE -ne 0) { throw 'DocFX metadata failed.' }
    & docfx build docfx.json --warningsAsErrors
    if ($LASTEXITCODE -ne 0) { throw 'DocFX build failed.' }
} finally {
    Pop-Location
}
