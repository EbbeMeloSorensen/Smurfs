# Smurfs

Smurfs is a small **.NET 10 reference solution for AI-assisted roundtrip engineering**. It explores how Codex can assist with requirements, modeling, implementation and verification while the human engineer retains authority over intended behavior and acceptance.

The project deliberately uses a simple domain model, allowing attention to remain on software architecture, maintainability, documentation, and the development workflow rather than domain complexity.

## Engineering approach

The solution follows a model-informed, code-first approach in which source code and architectural models are maintained together.

Key principles include:

- **Architecture:** Maintain clear responsibilities and dependencies between projects, layers, and components.
- **Roundtrip engineering:** Keep architectural models and source code synchronized as both evolve.
- **Documentation as code:** Maintain editable PlantUML models and documentation alongside source code, with generated SVG diagrams and API references.
- **AI-assisted development:** Use Codex to implement and document changes while keeping designs, decisions, and resulting code understandable and reviewable by developers.
- **Quality assurance:** Apply consistent coding conventions, static analysis, and automated testing to detect regressions and architectural inconsistencies.
- **Reproducibility:** Automate builds, tests, and documentation generation across supported development environments.

The architectural documentation separates structural and behavioral perspectives, complementing the API reference generated from C# source code and XML documentation comments.

## Project documentation

The following documents provide the starting points for understanding and maintaining the solution:

- **[AGENTS.md](AGENTS.md):** Instructions for AI-assisted development and maintaining consistency between code, models, and documentation.
- **[Requirements and use cases](docs/requirements/index.md):** FURPS++ specification, catalog goals, quality attributes, and approval status.
- **[Verification and validation](docs/testing/index.md):** integration scope, manual acceptance procedures and execution records.
- **[Engineering process](docs/engineering/index.md):** iterative V-model and traceability.
- **Engineering conventions:** Coding, architecture, documentation, and testing standards for the solution.
- **Project context:** Domain concepts, architectural boundaries, and design rationale.
- **Baseline review:** Verified architecture, validation results, known limitations, and outstanding decisions.
- **Structural and behavioral models:** Editable PlantUML diagrams describing the architecture and behavior.
- **Console startup and commands:** Instructions for configuring, starting, and interacting with the application, including paging.

Developers and AI coding agents should consult the relevant documentation before making changes.

## Build and test the solution

### Prerequisites

Install a stable **.NET 10 SDK**.

The `global.json` file at the solution root controls SDK selection, allowing compatible installed .NET 10 SDK versions according to its configured roll-forward policy.

Verify the selected SDK by running the following command from the repository root:

```powershell
dotnet --version
```

### Build

From the repository root, build the solution:

```powershell
dotnet build Smurfs.sln
```

### Run tests

Execute the automated tests:

```powershell
dotnet test Smurfs.sln
```

The test project retains its historical `UnitTest` name, although its current tests exercise persistence against real, in-memory SQLite databases and isolated on-disk JSON catalogs. These tests cover integration as well as isolated behavior.

SQLite tests use in-memory databases; file tests use temporary directories. SQLite migration tests use the AppData SQLite context and migration set. PostgreSQL migration discovery and snapshot agreement are checked without opening a database connection.

## Build the documentation

The documentation is generated using [DocFX](https://dotnet.github.io/docfx/) and [PlantUML](https://plantuml.com/). DocFX generates the API reference from C# source code and XML documentation comments, while PlantUML generates SVG diagrams from the structural, behavioral, and other architectural models.

### Prerequisites

Install the following tools:

| Tool | Purpose |
|---|---|
| .NET 10 SDK | Build the .NET projects and generate API metadata |
| Python 3.10+ | Validate traceability and generate the matrix |
| DocFX | Generate the documentation website |
| Java | Execute PlantUML |
| Graphviz | Render PlantUML diagrams |
| PlantUML JAR | Generate SVG diagrams from `.puml` files |
| PowerShell 7 (Windows only) | Execute the Windows documentation build script |

Verify that `python` (Windows) or `python3` (Linux), `dotnet`, `docfx`, `java`, and `dot` are available on your `PATH`.

Download a PlantUML JAR from the [official download page](https://plantuml.com/download). The MIT-licensed distribution is recommended.

### Build on Windows

Use **PowerShell 7 or later**. Windows PowerShell 5.1 is not supported because the build script uses .NET APIs unavailable in that version.

From the repository root, execute:

```powershell
./docs/build-docs.ps1 -PlantUmlJar 'C:\Tools\PlantUML\plantuml.jar'
```

Alternatively, set the `PLANTUML_JAR` environment variable:

```powershell
$env:PLANTUML_JAR = 'C:\Tools\PlantUML\plantuml.jar'
./docs/build-docs.ps1
```

### Build on Linux

From the repository root, execute:

```bash
bash docs/build-docs.sh /path/to/plantuml.jar
```

Alternatively, set the environment variable:

```bash
export PLANTUML_JAR=/path/to/plantuml.jar
bash docs/build-docs.sh
```

On Linux, the script also supports invoking `plantuml` directly from `PATH` when no JAR path is provided.

### Build process

The build scripts validate IDs and links in the traceability manifest and generate the DocFX matrix before rendering PlantUML. To run the check by itself, use `python docs/validate-traceability.py` on Windows or `python3 docs/validate-traceability.py` on Linux.

The build scripts perform the following operations:

1. **Discover PlantUML sources:** Recursively find `.puml` files beneath `docs/`, excluding generated directories.
2. **Validate diagrams:** Check the PlantUML sources for errors before rendering.
3. **Generate diagrams:** Render SVG files into `docs/_generated/diagrams/`, preserving the relative directory structure.
4. **Generate API documentation:** Use DocFX to extract metadata from the C# projects and XML documentation comments into `docs/api/`.
5. **Build the website:** Generate the complete documentation site in `docs/_site/`.

The source and generated directories are organized as follows:

```text
docs/
  requirements/         Requirements, use cases and traceability manifest
  structural/           Static design and storage models
  behavioral/           Workflow and interaction models
  testing/              Test plan, acceptance cases and execution records
  engineering/          V-model and engineering process
  api/                  Generated API metadata
  _generated/           Generated traceability matrix and SVG diagrams
  _site/                Generated DocFX website
```

The `_generated/`, `api/`, and `_site/` directories contain generated artifacts and should normally be excluded from version control.

### Preview the documentation

After a successful build, start the DocFX development server from the repository root:

```powershell
docfx serve docs/_site
```

Open the local address reported by DocFX in your browser, normally `http://localhost:8080`.

The server serves the previously generated site. It does not automatically regenerate diagrams or API documentation when source files change.

### Updating the documentation

Rerun the appropriate build script whenever PlantUML sources, XML documentation comments, or other documentation files change.

Both scripts should produce equivalent documentation on Windows and Linux. For reproducible results, use the same PlantUML and DocFX versions on both platforms.

### Further information

- [Documentation publishing guide](docs/documentation.md#local-build)
- [November 2026 persistence baseline](docs/baseline-2026-11-04.md)
## Populate a database for paging tests

The current seed configuration creates eight sample rows (seven named samples and one randomly named Smurf) on first initialization of an empty catalog. Select SQLite, PostgreSQL, or File in the console settings and use a disposable test database. See [console database settings](docs/behavioral/console.md#database-settings) for file locations and environment overrides.

```powershell
dotnet run --project src/Smurfs.UI.Console -- initdb
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 2 --page-size 25
```

The legacy `scripts/add-random-smurfs.ps1` inserts into a SQLite file. It cannot provision or populate the current PostgreSQL catalogs.

For API and WebClient deployment, see the API README at `src/Smurfs.API/README.md`.

## AppData and Identity providers and migrations

Each persistence assembly keeps its shared model in the abstract `DataContext`. `PostgresDataContext` and `SqliteDataContext` each have their own migrations and model snapshot in the same assembly, under `Migrations/Postgres` and `Migrations/Sqlite`.

WPF selects its AppData provider from `Database:Provider` and defaults to PostgreSQL. Its [database settings](docs/behavioral/wpf.md#database-settings) also support SQLite. Avalonia also selects its AppData provider from `Database:Provider` and defaults to PostgreSQL. WinForms also selects its AppData provider from `Database:Provider` and defaults to PostgreSQL. See [WinForms database settings](docs/behavioral/winforms.md#database-settings). The console selects its AppData provider from `Database:Provider` and defaults to SQLite. It also supports `File`; see [file persistence](docs/structural/index.md#file-persistence). The API selects both contexts from `Database:Provider` at startup, defaulting to the checked-in `Sqlite` setting. See `src/Smurfs.API/README.md` for connection settings and Heroku overrides. To compose an AppData host with SQLite, register `AddAppDataPersistence<SqliteDataContext>(options => options.UseSqlite(connectionString))`. The registration also exposes the selected context and its factory through the shared `DataContext` contract. Identity uses the same sibling provider context structure. Register its selected provider with `AddIdentityPersistence<SqliteDataContext>(options => options.UseSqlite(connectionString))` when composing a SQLite host. Both extension methods live in their respective persistence namespaces.

The JSON file adapter in `Smurfs.Persistence.File` implements the same unit-of-work/repository and initializer contracts. Select it in the console with `Database:Provider=File` and configure `FileStore:Path` (default `smurfs.json` beside the executable). For example:

```powershell
$env:Database__Provider = 'File'
$env:FileStore__Path = 'C:\SmurfsData\catalog.json'
dotnet run --project src/Smurfs.UI.Console -- initdb
dotnet run --project src/Smurfs.UI.Console -- createsmurf --name "File Smurf"
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page-size 5
```

The first command creates the directory/file and seeds eight rows when empty. Repeating it preserves existing Smurfs. File persistence needs no EF migrations or database connection. It uses a versioned JSON format, atomic replacement, and conflict detection; stale writers must reload and retry. SQLite and PostgreSQL remain supported, and the other frontends retain their current database settings. No data is transferred automatically between providers. See [file storage behavior](docs/structural/index.md#file-persistence) for format and concurrency limits. Other hosts can register `services.AddFilePersistence(filePath)` instead of their EF initializer, unit-of-work factory, and read-service registrations.

For each model change, generate one migration per provider in the affected persistence project from the repository root with the EF Core CLI:

```powershell
$project = 'src/Smurfs.Persistence.EFCore.AppData' # Use Smurfs.Persistence.EFCore.Identity for Identity changes
dotnet ef migrations add DescribePostgresChange --project $project --startup-project $project --context PostgresDataContext --output-dir Migrations/Postgres
dotnet ef migrations add DescribeSqliteChange --project $project --startup-project $project --context SqliteDataContext --output-dir Migrations/Sqlite
```

To ease this process for AppData changes, [scripts/add-appdata-migrations.sh](scripts/add-appdata-migrations.sh) runs both generation commands for you. Supply one DescribeChange argument; the script uses that name for both provider migrations in the AppData project:

```bash
bash scripts/add-appdata-migrations.sh RenameAgeToHealth
```

Run it using Bash on Linux/macOS, Git Bash, or WSL. It requires the .NET 10 SDK and a compatible EF Core 10 CLI tool on PATH. If needed, install the tool with:

```sh
dotnet tool install --global dotnet-ef --version 10.0.14
```

The script resolves the project relative to its own location, generates PostgreSQL first and SQLite second, and updates both model snapshots. Review the generated migrations before applying them, especially for a column rename where existing values should be preserved. If generation fails, inspect git diff before retrying because the first migration may already have been generated. Use the manual commands above for Identity changes.

The design-time factories use local development settings and do not open a connection when generating migrations. To apply a migration explicitly, pass `--connection` to `dotnet ef database update` with the chosen context. SQLite design-time defaults are `Data Source=smurfs-sqlite.db` for AppData and `Data Source=smurfs-identity-sqlite.db` for Identity. Start with a fresh file for each new baseline. The API uses one database for both contexts, selecting its provider from configuration. A SQLite host can explicitly supply the same SQLite connection to both registrations.

The existing PostgreSQL migration ID is preserved. Generating or switching migration sets does not transfer data between providers. Pass the project explicitly because both assemblies expose context types with the same names.

### Required creation timestamps

The Smurf model includes a required CreatedUtc value. AddedCreatedTimestampToSmurf migrations are included for PostgreSQL and SQLite. Both currently use the generated year-0001 default for existing rows. Seed data uses fixed UTC times starting at 2026-01-01T00:00:00Z, and new Smurfs receive their creation time from the application.

Apply the appropriate provider migration before running the updated clients against an existing database.
