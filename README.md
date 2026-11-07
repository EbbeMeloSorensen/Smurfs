# Smurfs

A small .NET 10 reference solution for round-trip engineering.

Start with [AGENTS.md](AGENTS.md), [engineering conventions](docs/roundtrip.md), and [project context](docs/project-context.md).

The [baseline review](docs/baseline-review.md) records verified architecture, validation and remaining decisions. Editable diagrams cover the [structural](docs/structural/index.md) and [behavioral](docs/behavioral/index.md) perspectives. See [console startup and commands](docs/behavioral/index.md#console-workflow) for first-run setup and paging.

Use a stable .NET 10 SDK. The solution root's `global.json` selects the highest installed stable .NET 10 feature band and patch. Run `dotnet --version` from the solution root to confirm the selection.

Build and run the SQLite integration tests:

```powershell
dotnet build Smurfs.sln
dotnet test Smurfs.sln
```

The test project retains its existing `UnitTest` name, although its current tests exercise real in-memory SQLite databases. Tests do not use `smurfs.db` or production data.

## Build the documentation

Use the .NET 10 SDK and make sure DocFX (`docfx`), Java (`java`), and Graphviz (`dot`) are on your PATH. Use a local PlantUML JAR and replace the example path below with its location.

From the repository root, run the command for your platform.

**Windows (PowerShell 7)**

```powershell
./docs/build-docs.ps1 -PlantUmlJar 'C:\path\to\plantuml.jar'
```

**Linux (Bash)**

```bash
bash docs/build-docs.sh /path/to/plantuml.jar
```

Both scripts accept the JAR path through the `PLANTUML_JAR` environment variable if you omit the path argument. On Linux, the script can also use `plantuml` on your PATH when neither JAR option is set.

The build validates the structural and behavioral PlantUML sources and regenerates their SVGs in `docs/_generated/diagrams/`. DocFX then generates the .NET API reference from C# and XML comments in `docs/api/` and publishes the complete site to `docs/_site/`. Rerun the script whenever you change a diagram or other documentation source.

Preview the generated site from the repository root.

```text
docfx serve docs/_site
```

See the [publishing guide](docs/documentation.md#local-build) and the [November 2026 persistence baseline](docs/baseline-2026-11-04.md).

## Populate a database for paging tests

Run from the directory where you want the local test database. From the repository root, for example:

```powershell
./scripts/add-random-smurfs.ps1 -Count 1000
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 2 --page-size 25
```

The script uses Python 3 on PATH for SQLite bulk inserts and the .NET console app to initialize a missing `smurfs.db`. It appends exactly the requested count in one transaction, in addition to the sample rows created on first use. Names can repeat so paging tests exercise the identifier tie-breaker.
