# Console workflows

The console runs one verb per process. Each diagram assumes argument parsing has selected the verb. After argument parsing selects a verb, the program opens its rolling file logger, builds the host, and opens a service scope. It disposes the scope and host before disposing the logger. Failures during startup are written to the file when the logger is available and reported to stderr with exit code 1. See [Application logging](../structural/index.md#application-logging). All verbs use the configured SQLite or PostgreSQL database. If that database is missing, `createsmurf` and `listsmurfs` run the same migration and seeding operations as `initdb` before dispatch. The diagrams refer to that shared behavior as database setup.

A use-case failure or caught runtime exception is printed to standard error and returns exit code 1. If first-use setup fails after creating the database, run `initdb` to retry. The console filters the MediatR license log category.

## Database settings

The console loads `appsettings.json` beside its executable, so it can be launched from any working directory. The project copies this file into build and publish output. Set `Database:Provider` to `Postgres` or `Sqlite` and configure `ConnectionStrings:Postgres` or `ConnectionStrings:Sqlite` respectively. The checked-in default targets PostgreSQL `Smurfs_ConsoleApp`, and SQLite defaults to `Data Source=smurfs-console.db` relative to the process working directory.

Edit the deployed settings file before the next invocation to switch providers without rebuilding. Environment variables override file settings, for example:

```powershell
$env:Database__Provider = 'Sqlite'
$env:ConnectionStrings__Sqlite = 'Data Source=smurfs-console.db'
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page-size 5
```

Provider names are case-insensitive. A missing or unsupported provider, or a missing connection string for the selected provider, produces a startup error and exit code 1. Each provider uses its own AppData migrations and database contents.

## initdb

Explicitly applies outstanding migrations and seeds seven named samples and 1,000 randomly named Smurfs when the Smurfs table is empty. It also runs against an existing database. This realizes [UC-001 Initialize local storage](../requirements/use-cases.md#uc-001-initialize-local-storage).

```powershell
dotnet run --project src/Smurfs.UI.Console -- initdb
```

![Console database initialization sequence](../_generated/diagrams/behavioral/diagrams/console-initdb.svg)

[PlantUML source](diagrams/console-initdb.puml)

## createsmurf

Submits a proposed name. The handler checks the draft through the business rule catalog, constructs a Smurf with age zero, checks the entity rules and commits in one write session. Names require non-whitespace text and have a limit of 20 characters. If a rule fails, the console displays the first violation. This realizes [UC-002 Create a Smurf](../requirements/use-cases.md#uc-002-create-a-smurf).

```powershell
dotnet run --project src/Smurfs.UI.Console -- createsmurf --name "New Smurf"
```

![Console create sequence](../_generated/diagrams/behavioral/diagrams/console-createsmurf.svg)

[PlantUML source](diagrams/console-createsmurf.puml)

## listsmurfs

Pages start at 1, with a default size of 25 and a maximum of 50. The read adapter counts rows and fetches the requested page ordered by name, then identifier. An out-of-range page is empty. Because count and page retrieval use separate queries, concurrent writes can change page contents or totals. This realizes [UC-003 List Smurfs](../requirements/use-cases.md#uc-003-list-smurfs).

```powershell
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 2 --page-size 5
```

![Console listing sequence](../_generated/diagrams/behavioral/diagrams/console-listsmurfs.svg)

[PlantUML source](diagrams/console-listsmurfs.puml)
