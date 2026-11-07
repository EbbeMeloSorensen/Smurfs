# Smurfs

Smurfs is a small **.NET 10 reference solution for AI-assisted roundtrip engineering**. Its purpose is to establish and evaluate engineering practices that keep source code, architectural models, documentation, and automated tests consistent as the solution evolves.

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

The test project retains its historical `UnitTest` name, although its current tests exercise persistence against real, in-memory SQLite databases. These tests therefore cover database integration rather than only isolated units of code.

The tests do not access `smurfs.db` or production data.

## Build the documentation

The documentation is generated using [DocFX](https://dotnet.github.io/docfx/) and [PlantUML](https://plantuml.com/). DocFX generates the API reference from C# source code and XML documentation comments, while PlantUML generates SVG diagrams from the structural, behavioral, and other architectural models.

### Prerequisites

Install the following tools:

| Tool | Purpose |
|---|---|
| .NET 10 SDK | Build the .NET projects and generate API metadata |
| DocFX | Generate the documentation website |
| Java | Execute PlantUML |
| Graphviz | Render PlantUML diagrams |
| PlantUML JAR | Generate SVG diagrams from `.puml` files |
| PowerShell 7 (Windows only) | Execute the Windows documentation build script |

Verify that `dotnet`, `docfx`, `java`, and `dot` are available on your `PATH`.

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

The build scripts perform the following operations:

1. **Discover PlantUML sources:** Recursively find `.puml` files beneath `docs/`, excluding generated directories.
2. **Validate diagrams:** Check the PlantUML sources for errors before rendering.
3. **Generate diagrams:** Render SVG files into `docs/_generated/diagrams/`, preserving the relative directory structure.
4. **Generate API documentation:** Use DocFX to extract metadata from the C# projects and XML documentation comments into `docs/api/`.
5. **Build the website:** Generate the complete documentation site in `docs/_site/`.

The source and generated directories are organized as follows:

```text
docs/
├── structural/
│   └── diagrams/
│       └── domain.puml
├── behavioral/
│   └── diagrams/
│       └── lifecycle.puml
├── _generated/
│   └── diagrams/
│       ├── structural/
│       │   └── diagrams/
│       │       └── domain.svg
│       └── behavioral/
│           └── diagrams/
│               └── lifecycle.svg
├── api/
├── _site/
├── build-docs.ps1
├── build-docs.sh
└── docfx.json
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

Run from the directory where you want the local test database. From the repository root, for example:

```powershell
./scripts/add-random-smurfs.ps1 -Count 1000
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 2 --page-size 25
```

The script uses Python 3 on PATH for SQLite bulk inserts and the .NET console app to initialize a missing `smurfs.db`. It appends exactly the requested count in one transaction, in addition to the sample rows created on first use. Names can repeat so paging tests exercise the identifier tie-breaker.
