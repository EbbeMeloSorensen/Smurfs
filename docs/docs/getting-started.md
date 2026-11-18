# Getting started

From the repository root:

```powershell
dotnet build Smurfs.sln
dotnet test Smurfs.sln
$env:PLANTUML_JAR = 'C:\path\to\plantuml.jar'
./docs/build-docs.ps1
```

To try the console, run the following from the repository root. The first invocation creates and seeds the local PostgreSQL database `Smurfs_ConsoleApp` if it is missing.

```powershell
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 1 --page-size 5
```

See the [console workflow](../behavioral/console.md) for creation and explicit initialization.

See [publishing prerequisites](../documentation.md#local-build) for Java, Graphviz and DocFX. Open docs/_site/index.html through a local HTTP server to inspect the generated site. The persistence tests use in-memory SQLite. See the [test plan](../testing/index.md) for the current migration coverage gap.
