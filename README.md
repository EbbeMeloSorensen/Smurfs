# Smurfs

A small .NET 10 reference solution for round-trip engineering.

Start with [AGENTS.md](AGENTS.md), [engineering conventions](docs/roundtrip.md), and [project context](docs/project-context.md).

The [baseline review](docs/baseline-review.md) records verified architecture, validation and remaining decisions. Editable diagrams cover the [structural](docs/structural/index.md) and [behavioral](docs/behavioral/index.md) perspectives. See [console startup and commands](docs/behavioral/index.md#console-workflow) for first-run setup and paging.

Build and run the SQLite integration tests:

```powershell
dotnet build Smurfs.sln
dotnet test Smurfs.sln
```

The test project retains its existing `UnitTest` name, although its current tests exercise real in-memory SQLite databases. Tests do not use `smurfs.db` or production data.

Build the perspective-based documentation with `./docs/build-docs.ps1`; see [publishing prerequisites](docs/documentation.md#local-build) and the [November 2026 persistence baseline](docs/baseline-2026-11-04.md).

## Populate a database for paging tests

Run from the directory where you want the local test database. From the repository root, for example:

```powershell
./scripts/add-random-smurfs.ps1 -Count 1000
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 2 --page-size 25
```

The script uses Python 3 on PATH for SQLite bulk inserts and the .NET console app to initialize a missing `smurfs.db`. It appends exactly the requested count in one transaction, in addition to the sample rows created on first use. Names can repeat so paging tests exercise the identifier tie-breaker.
