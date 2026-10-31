# Smurfs

A small .NET 10 reference solution for round-trip engineering.

Start with [AGENTS.md](AGENTS.md), [engineering conventions](docs/roundtrip.md), and [project context](docs/project-context.md).

The [baseline review](docs/baseline-review.md) records verified architecture, validation and remaining decisions. Editable diagrams live under [docs/architecture](docs/architecture): conceptual domain, persistence architecture and relational model.

Build and run the SQLite integration tests:

```powershell
dotnet build Smurfs.sln
dotnet test Smurfs.sln
```

The test project retains its existing `UnitTest` name, although its current tests exercise real in-memory SQLite databases. Tests do not use `smurfs.db` or production data.
