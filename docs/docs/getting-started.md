# Getting started

From the repository root:

```powershell
dotnet build Smurfs.sln
dotnet test Smurfs.sln
$env:PLANTUML_JAR = 'C:\path\to\plantuml.jar'
./docs/build-docs.ps1
```

See [publishing prerequisites](../documentation.md#local-build) for Java, Graphviz and DocFX. Open docs/_site/index.html through a local HTTP server to inspect the generated site. Tests use isolated in-memory SQLite, not production data.
