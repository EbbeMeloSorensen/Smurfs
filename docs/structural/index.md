# Structural perspective

The [traceability matrix](../_generated/traceability.md) connects the design views below to requirements and evidence. The [use case model](../requirements/use-cases.md) describes the externally visible goals.

## Project dependencies

This solution-wide view shows the production and test projects in `Smurfs.sln`. An arrow points from a project to a project it directly references in its `.csproj` file. The diagram includes explicit references even when a transitive route also exists. Package dependencies and type relationships are covered separately or outside this view.

![Smurfs solution project dependencies](../_generated/diagrams/structural/diagrams/project-dependencies.svg)

[PlantUML source](diagrams/project-dependencies.puml)

See [Projects](../_generated/projects/index.md) for project responsibilities and direct project and NuGet references.

## Conceptual domain

[Smurf](xref:Smurfs.Domain.Smurf), [Pet](xref:Smurfs.Domain.Pet), and [Village](xref:Smurfs.Domain.Village) are public domain classes. This selective class model omits storage identifiers. Assigning a Smurf name requires non-whitespace text and at most 20 characters. Health has no range validation. A Smurf has an optional Pet; multiple Smurfs may share it. Village exposes a mutable collection. [ADR 0001](../decisions/0001-village-membership.md) establishes optional single-village membership and navigation from Village to Smurfs only. Plain CLR collections still permit conflicting memberships, a known implementation limitation. EF tracking represents membership with a single optional shadow VillageId foreign key; Smurf has no reverse village reference.

![Conceptual domain model](../_generated/diagrams/structural/diagrams/domain-model.svg)

Source: [domain-model.puml](diagrams/domain-model.puml).

## Persistence architecture

The assemblies in this view separate domain types, provider-neutral Craft and Smurfs contracts, and reusable/Smurfs-specific EF Core implementations. The diagram groups selected types by their containing assembly using UML components, rather than namespace packages. Arrows describe type relationships; those crossing component boundaries reveal the relevant assembly dependencies. The diagram does not enumerate project references. Explicit domain dependencies show generic type arguments, entity sets and configuration targets; dependencies through other interfaces are not repeated as direct domain dependencies. The view focuses on the EF adapters and their Smurfs/Craft contracts. The File adapter has a separate view below. Standalone EF Core and BCL types and their connecting arrows are omitted; framework names remain in signatures where useful, such as DbSet<Smurf>. Framework inheritance and interface contracts remain available in the API reference. Domain members and relationships remain detailed in the conceptual domain model. Only the Smurf repository exists. In the EF adapter, one UnitOfWork owns the shared DataContext, saves it on CompleteAsync and disposes it asynchronously. The abstract DataContext owns the shared model. PostgresDataContext and SqliteDataContext inherit that model and have separate migrations and snapshots in the AppData assembly. Each has its own design-time factory. AppDataContextFactory adapts the selected runtime provider factory to IDbContextFactory<DataContext> for the existing unit-of-work service.

![Persistence architecture](../_generated/diagrams/structural/diagrams/persistence-architecture.svg)

Source: [persistence-architecture.puml](diagrams/persistence-architecture.puml). See [behavior](../behavioral/index.md) and [DataContext](xref:Smurfs.Persistence.EFCore.AppData.DataContext).

## File persistence

`Smurfs.Persistence.File` is an alternative catalog adapter. It references the persistence contracts and the application-owned read contract; neither Domain nor Application depends on it. `AddFilePersistence(path)` registers a singleton factory and initializer and a scoped read service. Each factory call creates an independent, lazily loaded snapshot. Sessions and their tracked entities are not thread-safe. A repository query returns tracked objects, so the shared Edit handler can mutate them and commit without calling UpdateAsync. Queries combine predicates with AND and include changes staged in the same session. Disposing discards uncommitted state. Detached UpdateAsync requires an existing ID; AddAsync rejects duplicate IDs.

![File persistence architecture](../_generated/diagrams/structural/diagrams/file-persistence.svg)

Source: [file-persistence.puml](diagrams/file-persistence.puml). This scoped view shows the public contracts and principal implementation types; internal JSON record helpers are omitted. Smurf and Pet have constructors accepting a nonempty existing identifier for rehydration. Their default constructors still generate new identifiers. The conceptual relationships and EF relational schema are unchanged; no migrations were added.

The UTF-8 JSON document has `version` (currently 1), `revision`, and separate `smurfs`, `pets`, and `villages` arrays. Smurf records contain every scalar field, a pet ID, and an optional village ID. Required fields include explicit nulls; Gender is a string when present, and CreatedUtc must be UTC. Shared pets are restored as shared objects. Village records and membership IDs are preserved across commits, but there is no Village/Pet management repository or console command. The shared `SampleSmurfs.Create()` supplies the same eight sample rows to EF and File initializers. `MigrateAsync` creates or validates the file; unsupported versions and malformed or inconsistent documents fail without replacing data. No historical file format or automatic database export/import is provided.

`CompleteAsync` acquires an exclusive sidecar `.lock` handle, compares the snapshot revision with the current document, writes and flushes a temporary file in the same directory, and replaces the catalog by rename. Cancellation or serialization failure before replacement leaves the previous document intact. An out-of-date session throws `FileConflictException`; its caller must start a new session, reload, and retry deliberately. The initializer retries a seeding conflict after rechecking whether the catalog is still empty. Lock acquisition retries for up to five seconds. The `.lock` file normally remains on disk; ownership is the open handle, not its existence. Readers see the previous or new complete document. After process termination, an orphaned temporary file may remain; it is not used as the catalog.

This provider loads and rewrites the complete catalog and targets small local stores on a filesystem supporting exclusive handles and same-directory atomic rename. It does not coordinate manual edits, synchronize machines, provide database-scale querying, or claim crash durability on arbitrary/network filesystems. Back up the JSON file after writes have stopped. Name matching uses ordinal case-insensitive literal substring matching and alphabetical ordering uses ordinal comparison; database collation can produce a different name ordering for some characters. Identifier tie-breaking and newest-first sorting remain deterministic.

## Console use cases

The console is the composition root for Application, the selected persistence adapter and Infrastructure logging. Application owns the MediatR requests, handlers and read contract. It depends on provider-neutral persistence contracts for writes. Infrastructure implements the read contract using the EF app-data context, with ordering and DTO projection in SQL. The EF read and write paths share the app-data database. Selecting File registers its repository, factory, initializer, and read adapter instead.

The following view shows EF composition; the File adapter types and contract implementations are shown in the file-persistence view above.

![Console use-case architecture](../_generated/diagrams/structural/diagrams/console-architecture.svg)

The [console architecture source](diagrams/console-architecture.puml) shows selected type dependencies grouped by assembly. The persistence architecture view covers the underlying repository and session types. The generic paging adapter remains available outside the active list path. The validation pipeline is registered, and the console currently registers no FluentValidation request validators. The create handler uses the scoped domain business rule catalog. Craft.Domain supplies the rule contracts and catalog base. Smurfs.Domain registers the name rule. The catalog evaluates atomic rules for a single entity and supports rules over related entities. The current Smurfs catalog registers the same name rule for drafts and entities. Rules return [violation values](xref:Craft.Domain.BusinessRuleViolation), so multiple rules can report failures for the same member.

## Application logging

This design supports [SR-001 Diagnose failures after exit](../requirements/index.md#sr-001-diagnose-failures-after-exit).

The create, edit, delete, and list handlers use typed `ILogger<T>` instances from the host. Successful operations log at Information, and handled input rejections log at Debug. A MediatR behavior logs unexpected handler exceptions at Error and propagates them to the caller. The handlers log operational details such as page and row counts and omit draft names and request payloads.

WPF and the console initialize the shared file logger in `Smurfs.Infrastructure` before building their hosts. When available, the logger records startup failures even if the host could not be built. It writes plain-text files to `%LOCALAPPDATA%\Smurfs\Logs\wpf-YYYYMMDD.log` and `%LOCALAPPDATA%\Smurfs\Logs\console-YYYYMMDD.log`. Each entry includes the timestamp, level, category, message, and any exception details. Avalonia and WinForms also use the shared logger, with the `avalonia-` and `winforms-` prefixes. Each frontend rolls files daily or at 5 MiB and retains its latest 14 files. Smurfs categories default to Information and other categories to Warning. Set `Logging__LogLevel__Smurfs=Debug` before launching either frontend for more detail. WPF keeps debugger output, and the console keeps diagnostics on stderr and command results on stdout.

## Identity storage

The separate Identity adapter defines an EF context for ASP.NET Core Identity and adds a required `DisplayName` to `AspNetUsers`. The initial Identity migration `20261116175127_InitialMigration` creates the user and role tables and their related claim, login, membership and token tables. The abstract Identity DataContext shares its model with PostgresDataContext and SqliteDataContext. Each provider has a migration folder, snapshot, and design-time factory in the Identity assembly. The PostgreSQL baseline retains its existing migration ID. AddIdentityPersistence registers the selected concrete context and exposes that same scoped instance as DataContext for Identity stores and startup migration. The console currently uses app-data services only and does not register the Identity context, user accessor or sample account seeder.

## Relational model

This app-data model reflects the current EF runtime model: Smurfs, Villages and Pets tables. DataContext exposes all three entity sets. Smurf has optional indexed PetId and shadow VillageId foreign keys; each village and pet can be associated with multiple Smurfs. VillageId is nullable and maintained by EF, with no corresponding C# property. SmurfConfiguration explicitly maps the optional Pet relationship with Restrict. SmurfConfiguration also explicitly maps optional Village membership through the shadow VillageId, with ClientSetNull and navigation from Village to Smurfs only. DataContext.OnModelCreating calls the shared Configure helper, which applies SmurfConfiguration. See [ADR 0002](../decisions/0002-pet-configuration.md) for the mapping and deletion semantics.

EF Core can materialize legacy Smurf names through the backing field. The PostgreSQL Name column requires a value but does not enforce the domain's whitespace or length checks.

AppData retains the PostgreSQL baseline 20261118194640_InitialPostgres and SQLite baseline 20261118192145_InitialSqlite. Each provider now also includes ChangedAgeToHealth, GenderAndInfected (which adds Birthday as well), and AddedCreatedTimestampToSmurf migrations. Both current snapshots contain Health, Birthday, Infected, Gender, and CreatedUtc. The Age column belongs to the historical initial schema and is renamed by the later migration. The latest CreatedUtc migrations use the generated year-0001 default for existing rows. Presence in source does not establish which migrations have been applied to a particular database. SQLite migration application is exercised in-memory; live PostgreSQL application remains a separate check.

![Relational persistence model](../_generated/diagrams/structural/diagrams/persistence-model.svg)

Source: [persistence-model.puml](diagrams/persistence-model.puml). [Unresolved questions](../decisions/index.md) require review before implementation changes.

## WPF client

The WPF application composes the same list and create handlers, read adapter, domain rule catalog and app-data initializer as the console. Its create dialog uses a scoped view model to validate the draft through the catalog and send the application command. The main window binds to a view model that depends on an application request and a DI scope factory. The view model resolves the sender per load, so the scoped read adapter and EF context belong to the load operation. The [WPF workflows](../behavioral/wpf.md) show the startup and request lifetimes.

## HTTP API storage

Identity owns the account tables and AppData owns the catalog tables. Smurfs.API selects both provider contexts using `Database:Provider` and applies their migration sets to the same database. The console selects its AppData provider from settings and defaults to SQLite and can also select a JSON file catalog. WPF also selects its AppData provider from settings, defaulting to `Smurfs_WPF` on PostgreSQL. Avalonia selects its provider from settings and defaults to `Smurfs_Avalonia` on PostgreSQL. WinForms selects its AppData provider from settings and defaults to `Smurfs_WinForms` on PostgreSQL.

## Editing a Smurf

The application-owned Edit.Command identifies an existing Smurf by Id and supplies replacement Name, Health, Birthday, Infected, and Gender values. The handler checks the name draft through the domain catalog, loads the tracked entity in a unit of work, applies the values, and validates the resulting entity before committing. Missing entities and catalog violations return failures. Health follows the domain's current unrestricted integer contract. Existing relationships remain attached to the loaded entity. Cancellation and persistence exceptions propagate through the shared request failure logging pipeline.

The command returns Result<Unit>, matching Create. MediatR discovers the handler through AddApplication(). The API exposes PUT /api/smurfs/{id} with a JSON body containing name, health, birthday, infected, and gender. The route supplies the identifier. The endpoint uses the shared result mapping, returning 200 on success and 400 for a handler failure, including a missing Smurf. Desktop edit screens are not implemented. EditSmurfTests verifies successful persistence, missing IDs, name and entity rejection without partial writes, and preservation of the pet relationship.

## Deleting a Smurf

Delete.Command identifies an existing Smurf by Id. The handler loads the entity within a unit of work, stages removal through the repository, and commits before returning Result<Unit>. A missing identifier returns the same failure message as Edit. Cancellation and persistence exceptions propagate through the existing logging pipeline. Deletion removes the selected Smurf and preserves associated Pet and Village records.

The authenticated API exposes DELETE /api/smurfs/{id} with no request body. The controller uses the existing result mapping, returning 200 on success and 400 if the Smurf is missing. DeleteSmurfTests verifies persisted removal, preservation of another Smurf and a shared pet, missing identifiers, and cancellation.

## Optional domain values

Smurf.Birthday is a nullable DateTime. Infected is a nullable bool, and Gender is a nullable domain enum with Male and Female values. Null represents an unknown value. EF Core maps Gender to a nullable integer by convention. Both providers include migrations and matching snapshots for these properties.

## Smurf list response fields

The paged read DTO exposes id, name, createdUtc, health, birthday, infected, gender, and petId. GET /api/smurfs includes nullable properties explicitly as JSON null when unknown. Gender is serialized as Male or Female text. Infection status distinguishes true, false, and null. PetId identifies the related pet without serializing its navigation graph. SmurfReadService projects these fields directly from the database.

## Creation timestamps and list ordering

Smurf.CreatedUtc is a non-nullable DateTime initialized from UTC time when a new Smurf is constructed. Seed/import code can supply it during initialization. Edit commands preserve it, and API create/edit contracts do not accept it. Read DTOs expose createdUtc. Persistence marks restored values as UTC so JSON includes the UTC suffix.

Seed rows have fixed creation times starting at 2026-01-01T00:00:00Z, increasing by one minute per row. Their names may remain random. Both list sort modes execute before paging: name ascending then identifier, or CreatedUtc descending then identifier. The identifier resolves equal timestamps consistently. The WebClient retains the server order.

AddedCreatedTimestampToSmurf migrations and updated snapshots are included for both providers. The migrations currently use the generated year-0001 default for historical rows. Rows sharing a timestamp use the identifier tie-breaker. Historical creation dates cannot be reconstructed from these defaults. Apply the appropriate migration before running against an existing database.
