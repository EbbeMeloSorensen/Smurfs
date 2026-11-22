# Smurfs project context (project-specific)

> This file states design intent learned so far. The checked-in code and EF Core configurations are the source of truth for **implemented** behavior. Reconcile rather than assume when they disagree.

## Purpose

Smurfs is a small C#/.NET learning/reference solution for AI-assisted roundtrip engineering. It deliberately exercises code-first domain modeling, PlantUML design models, repositories, unit of work, XML documentation and DocFX. Keep the solution understandable; do not add frameworks or complexity merely to demonstrate tooling.

## Actors and verification

The catalog operator initializes local storage, creates a Smurf and lists stored Smurfs through the console. The authenticated HTTP API and React client also support creation, detail retrieval, editing, deletion, filtering, and sorting. The WPF client supports creating a Smurf and browsing adjacent pages through the same application use cases. The [use case specifications](requirements/use-cases.md) distinguish proposed intent from current behavior. Village, Pet and their associations have persistence coverage and no console management workflow. The [test plan](testing/index.md) pairs in-memory SQLite integration tests with manual console and WPF cases on a disposable PostgreSQL instance. Use the [requirements specification](requirements/index.md) for approval status and the generated matrix for evidence links.

## Domain intent

- `Smurf`, `Village`, and `Pet` are domain concepts.
- A `Village` can contain zero or many `Smurf` instances; each `Smurf` belongs to zero or one `Village`. This is accepted in [ADR 0001](decisions/0001-village-membership.md). Navigation goes from Village to Smurfs only; Smurf has no Village navigation. The relational model uses nullable `Smurfs.VillageId` referencing `Villages.Id`, explicitly mapped in SmurfConfiguration as an EF shadow property with ClientSetNull.
- A `Smurf` has zero or one `Pet`; a `Pet` may be shared by multiple `Smurf` instances. [ADR 0002](decisions/0002-pet-configuration.md) records explicit Restrict mapping and its tracked/untracked deletion semantics.
- Smurf name assignments require non-whitespace text and at most 20 characters. The WPF dialog and shared create handler validate proposed names through the domain catalog. The handler constructs the Smurf and validates the entity before persistence.
- Distinguish optional navigation properties from required names or other invariants. Check actual definitions and EF Core mappings before documenting exact field/property names or cascade behavior.
- Keep conceptual class models free of PK/FK stereotypes where possible. Separate persistence/ER representations may show keys and foreign keys.

## Architecture intent

For behavioral modeling, document one sequence per console verb and describe shared console setup once. In WPF, model the application lifetime from launch through idle to shutdown without intervening operator actions. Model creation and browsing in separate diagrams, and keep startup failure separate from the successful lifetime. Link both clients to the same use case specifications.


- Keep domain and application contracts independent of EF Core infrastructure.
- The console composes application handlers, the app-data initializer and write session factory, and the selected read service (Infrastructure for EF, File for JSON). The command persists a domain object through the unit of work. The query retrieves projected summaries through an application-owned read contract.
- Console startup selects the AppData provider from settings and creates its selected storage on first use. Its checked-in default is SQLite; PostgreSQL and File are alternatives. The list operation queries the configured catalog. The Identity adapter has its own context and migration set and is outside the current console workflow.
- A persistence-ignorant `IRepository<TEntity>` lives in Craft.Persistence; EF Core-specific implementations live in Craft.Persistence.EFCore or solution-specific infrastructure.
- The Smurfs persistence boundary includes repository interfaces and a unit-of-work abstraction/factory. Check the actual current interfaces and names rather than inventing method signatures.
- The File adapter supplies the same persistence and application read contracts using independent JSON snapshots. Its selected architecture and concurrency limits are documented in the structural file-persistence section. Preserve project boundaries among Smurfs.Domain, Smurfs.Persistence, Smurfs.Persistence.EFCore.AppData and Craft dependencies as actually present.
- Scope the persistence architecture diagram to solution-owned types grouped by assembly components. Keep relevant direct dependencies between these types; omit standalone EF Core/BCL types and their arrows. Framework names may remain in member signatures. Do not add a shared external framework hub as the solution grows.
- Both AppData and Identity use an abstract DataContext with sibling PostgresDataContext and SqliteDataContext types. Each provider owns a migration set and snapshot in its respective persistence assembly. The API, console, WPF, Avalonia, and WinForms select provider contexts from settings. SQLite tests select SqliteDataContext.
- DataContext.OnModelCreating calls Configure to apply SmurfConfiguration; Smurf key, Pet and Village relationship rules belong in that configuration.
- EF Core is code-first. No migrations, database schema or storage behavior should be inferred purely from PlantUML.

## Domain-model changes

If a change affects classes, property types, associations, multiplicities or navigation/foreign-key relationships, inspect and reconcile `docs/structural/diagrams/domain-model.puml` and the relevant structural narrative. If persistence architecture changes, reconcile the persistence diagram. New domain types, repository specializations, entity sets and entity configurations also require reviewing direct domain dependencies in `docs/structural/diagrams/persistence-architecture.puml`, following the assembly component conventions in `roundtrip.md`. For example, a repository bound to Smurf depends on Smurf; DataContext depends on each entity type it exposes; a configuration depends on its target entity. A unit-of-work interface exposing only a repository does not thereby acquire a direct dependency on every domain type used by that repository. Inspect the current source rather than treating these examples as an exhaustive list.

Reconcile `docs/structural/diagrams/persistence-model.puml` against both the runtime EF model and the checked-in migration/snapshot, including table names, nullable and shadow foreign keys, indexes and delete behavior. Record differences explicitly. When migrations are regenerated, update any discrepancy documentation and tests that previously expected pending model changes. Behavioral diagrams should only be added for meaningful interactions or workflows, not fabricated to populate the documentation tree.

## API console milestone

Smurfs.UI.Console.API executes one command per process: login, logout, listsmurfs, and createsmurf. Catalog commands match the direct console's names/options/output and never prompt. Windows Credential Manager retains an endpoint-scoped token; SMURFS_API_TOKEN overrides it for unattended/portable execution. Passwords are not saved. Login is interactive unless --email and --password-stdin are supplied. Rejected credentials fail without prompting or replaying. Logout removes local credentials only. The HTTP adapter retains an in-memory token supplied/exported explicitly by its host. The server validates and commits creation, and uncertain outcomes are not automatically retried. The client has no Identity persistence/EF dependency and does not initialize server storage. Its repository/unit-of-work classes remain unregistered skeletons. Dataset import is a later server-side milestone. See behavioral/api-console.md for script conventions.

## HTTP API baseline

Smurfs.API composes Identity, JWT bearer authentication and the shared MediatR catalog handlers. Its two EF Core contexts share a database selected by `Database:Provider`, using matching SQLite or PostgreSQL context types and migration sets. The API also hosts the production WebClient. See the [HTTP API workflows](behavioral/http-api.md).

Identity persistence is referenced directly by the API. The HTTP user accessor lives in Smurfs.API.Services and implements the application-owned IUserAccessor contract. Shared Infrastructure supplies catalog reads and logging without depending on Identity persistence or HTTP context APIs.

The current seed count is eight rows. Each has a distinct fixed CreatedUtc timestamp starting at 2026-01-01T00:00:00Z. The application constructs new Smurfs with the current UTC creation time. Editing preserves it. Sorting and filtering execute before database paging. Provider snapshots include the Health rename, nullable Birthday/Infected/Gender, and required CreatedUtc; migration files are present for both providers.
