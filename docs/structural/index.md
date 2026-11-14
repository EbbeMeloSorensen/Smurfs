# Structural perspective

The [traceability matrix](../_generated/traceability.md) connects the design views below to requirements and evidence. The [use case model](../requirements/use-cases.md) describes the externally visible goals.

## Project dependencies

This solution-wide view shows the production and test projects in `Smurfs.sln`. An arrow points from a project to a project it directly references in its `.csproj` file. The diagram includes explicit references even when a transitive route also exists. Package dependencies and type relationships are covered separately or outside this view.

![Smurfs solution project dependencies](../_generated/diagrams/structural/diagrams/project-dependencies.svg)

[PlantUML source](diagrams/project-dependencies.puml)

See [Projects](../_generated/projects/index.md) for project responsibilities and direct project and NuGet references.

## Conceptual domain

[Smurf](xref:Smurfs.Domain.Smurf), [Pet](xref:Smurfs.Domain.Pet), and [Village](xref:Smurfs.Domain.Village) are public domain classes. This selective class model omits storage identifiers. Assigning a Smurf name requires non-whitespace text and at most 20 characters. Age has no range validation. A Smurf has an optional Pet; multiple Smurfs may share it. Village exposes a mutable collection. [ADR 0001](../decisions/0001-village-membership.md) establishes optional single-village membership and navigation from Village to Smurfs only. Plain CLR collections still permit conflicting memberships, a known implementation limitation. EF tracking represents membership with a single optional shadow VillageId foreign key; Smurf has no reverse village reference.

![Conceptual domain model](../_generated/diagrams/structural/diagrams/domain-model.svg)

Source: [domain-model.puml](diagrams/domain-model.puml).

## Persistence architecture

The five projects in this view separate domain types, provider-neutral Craft and Smurfs contracts, and reusable/Smurfs-specific EF Core implementations. The diagram groups selected types by their containing assembly using UML components, rather than namespace packages. Arrows describe type relationships; those crossing component boundaries reveal the relevant assembly dependencies. The diagram does not enumerate project references. Explicit domain dependencies show generic type arguments, entity sets and configuration targets; dependencies through other interfaces are not repeated as direct domain dependencies. The view focuses on solution-owned types. Standalone EF Core and BCL types and their connecting arrows are omitted; framework names remain in signatures where useful, such as DbSet<Smurf>. Framework inheritance and interface contracts remain available in the API reference. Domain members and relationships remain detailed in the conceptual domain model. Only the Smurf repository exists. One UnitOfWork owns the shared DataContext, saves it on CompleteAsync and disposes it asynchronously. The runtime factory and design-time SQLite factory have different purposes.

![Persistence architecture](../_generated/diagrams/structural/diagrams/persistence-architecture.svg)

Source: [persistence-architecture.puml](diagrams/persistence-architecture.puml). See [behavior](../behavioral/index.md) and [DataContext](xref:Smurfs.Persistence.EFCore.AppData.DataContext).

## Console use cases

The console is the composition root for Application, the EF app-data adapter and Infrastructure. Application owns the MediatR requests, handlers and read contract. It depends on provider-neutral persistence contracts for writes. Infrastructure implements the read contract using the EF app-data context, with ordering and DTO projection in SQL. The two paths share the app-data database.

![Console use-case architecture](../_generated/diagrams/structural/diagrams/console-architecture.svg)

The [console architecture source](diagrams/console-architecture.puml) shows selected type dependencies grouped by assembly. The persistence architecture view covers the underlying repository and session types. The generic paging adapter and AutoMapper profile remain available outside the active list path. The validation pipeline is registered, and the console currently registers no FluentValidation request validators. The create handler uses the scoped domain business rule catalog. Craft.Domain supplies the rule contracts and catalog base. Smurfs.Domain registers the name rule. The catalog evaluates atomic rules for a single entity and supports rules over related entities. The current Smurfs catalog registers the same name rule for drafts and entities. Rules return [violation values](xref:Craft.Domain.BusinessRuleViolation), so multiple rules can report failures for the same member.

## Application logging

The create and list handlers use typed `ILogger<T>` instances from the host. Successful operations log at Information, and handled input rejections log at Debug. A MediatR behavior logs unexpected handler exceptions at Error and propagates them to the caller. The handlers log operational details such as page and row counts and omit draft names and request payloads.

WPF and the console initialize the shared file logger in `Smurfs.Infrastructure` before building their hosts. When available, the logger records startup failures even if the host could not be built. It writes plain-text files to `%LOCALAPPDATA%\Smurfs\Logs\wpf-YYYYMMDD.log` and `%LOCALAPPDATA%\Smurfs\Logs\console-YYYYMMDD.log`. Each entry includes the timestamp, level, category, message, and any exception details. Each frontend rolls files daily or at 5 MiB and retains its latest 14 files. Smurfs categories default to Information and other categories to Warning. Set `Logging__LogLevel__Smurfs=Debug` before launching either frontend for more detail. WPF keeps debugger output, and the console keeps diagnostics on stderr and command results on stdout.

## Identity storage

The separate Identity adapter defines an EF context for ASP.NET Core Identity and adds a required `DisplayName` to `AspNetUsers`. The initial Identity migration `20261105171045_InitialMigration` creates the user and role tables and their related claim, login, membership and token tables. The design-time factory targets `smurfs-identity.db`. The console currently uses app-data services only and does not register the Identity context, user accessor or sample account seeder.

## Relational model

This app-data model reflects the current EF runtime model: Smurfs, Villages and Pets tables. DataContext exposes all three entity sets. Smurf has optional indexed PetId and shadow VillageId foreign keys; each village and pet can be associated with multiple Smurfs. VillageId is nullable and maintained by EF, with no corresponding C# property. SmurfConfiguration explicitly maps the optional Pet relationship with Restrict. SmurfConfiguration also explicitly maps optional Village membership through the shadow VillageId, with ClientSetNull and navigation from Village to Smurfs only. DataContext.OnModelCreating calls the shared Configure helper, which applies SmurfConfiguration. See [ADR 0002](../decisions/0002-pet-configuration.md) for the mapping and deletion semantics.

EF Core can materialize legacy Smurf names through the backing field. The SQLite Name column requires a value but does not enforce the domain’s whitespace or length checks.

The regenerated initial migration `20261104192233_InitialMigration` and snapshot now match this model: Smurfs, Pets and Villages, with nullable indexed PetId and VillageId foreign keys. Both foreign keys reference the corresponding Id primary key. The Pet foreign key specifies ON DELETE RESTRICT; the Village foreign key specifies no cascading database delete action. This replaces the earlier initial migration; it creates the current schema directly rather than renaming an existing Pet table. This reconciliation validates migration application only against isolated in-memory SQLite databases; no existing database was updated.

![Relational persistence model](../_generated/diagrams/structural/diagrams/persistence-model.svg)

Source: [persistence-model.puml](diagrams/persistence-model.puml). [Unresolved questions](../decisions/index.md) require review before implementation changes.

## WPF client

The WPF application composes the same list and create handlers, read adapter, domain rule catalog and app-data initializer as the console. Its create dialog uses a scoped view model to validate the draft through the catalog and send the application command. The main window binds to a view model that depends on an application request and a DI scope factory. The view model resolves the sender per load, so the scoped read adapter and EF context belong to the load operation. The [WPF workflows](../behavioral/wpf.md) show the startup and request lifetimes.
