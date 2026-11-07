# Structural perspective

## Conceptual domain

[Smurf](xref:Smurfs.Domain.Smurf), [Pet](xref:Smurfs.Domain.Pet), and [Village](xref:Smurfs.Domain.Village) are public domain classes. This selective class model omits storage identifiers. A Smurf has an optional Pet; multiple Smurfs may share it. Village exposes a mutable collection. [ADR 0001](../decisions/0001-village-membership.md) establishes optional single-village membership and navigation from Village to Smurfs only. Plain CLR collections still permit conflicting memberships, a known implementation limitation. EF tracking represents membership with a single optional shadow VillageId foreign key; Smurf has no reverse village reference.

![Conceptual domain model](../_generated/diagrams/structural/diagrams/domain-model.svg)

Source: [domain-model.puml](diagrams/domain-model.puml).

## Persistence architecture

The five projects in this view separate domain types, provider-neutral Craft and Smurfs contracts, and reusable/Smurfs-specific EF Core implementations. The diagram groups selected types by their containing assembly using UML components, rather than namespace packages. Arrows describe type relationships; those crossing component boundaries reveal the relevant assembly dependencies. The diagram does not enumerate project references. Explicit domain dependencies show generic type arguments, entity sets and configuration targets; dependencies through other interfaces are not repeated as direct domain dependencies. The view focuses on solution-owned types. Standalone EF Core and BCL types and their connecting arrows are omitted; framework names remain in signatures where useful, such as DbSet<Smurf>. Framework inheritance and interface contracts remain available in the API reference. Domain members and relationships remain detailed in the conceptual domain model. Only the Smurf repository exists. One UnitOfWork owns the shared DataContext, saves it on CompleteAsync and disposes it asynchronously. The runtime factory and design-time SQLite factory have different purposes.

![Persistence architecture](../_generated/diagrams/structural/diagrams/persistence-architecture.svg)

Source: [persistence-architecture.puml](diagrams/persistence-architecture.puml). See [behavior](../behavioral/index.md) and [DataContext](xref:Smurfs.Persistence.EFCore.AppData.DataContext).

## Console use cases

The console is the composition root for Application, the EF app-data adapter and Infrastructure. Application owns the creation request and the Smurf read contract. Infrastructure implements that contract using the app-data context, with ordering and DTO projection in the database. The persistence diagram below the conceptual model scopes its type relationships to the repository and unit-of-work design.

## Relational model

This separate model reflects the current EF runtime model: Smurfs, Villages and Pets tables. DataContext exposes all three entity sets. Smurf has optional indexed PetId and shadow VillageId foreign keys; each village and pet can be associated with multiple Smurfs. VillageId is nullable and maintained by EF, with no corresponding C# property. SmurfConfiguration explicitly maps the optional Pet relationship with Restrict. SmurfConfiguration also explicitly maps optional Village membership through the shadow VillageId, with ClientSetNull and navigation from Village to Smurfs only. DataContext.OnModelCreating calls the shared Configure helper, which applies SmurfConfiguration. See [ADR 0002](../decisions/0002-pet-configuration.md) for the mapping and deletion semantics.

The regenerated initial migration `20261104192233_InitialMigration` and snapshot now match this model: Smurfs, Pets and Villages, with nullable indexed PetId and VillageId foreign keys. Both foreign keys reference the corresponding Id primary key. The Pet foreign key specifies ON DELETE RESTRICT; the Village foreign key specifies no cascading database delete action. This replaces the earlier initial migration; it creates the current schema directly rather than renaming an existing Pet table. This reconciliation validates migration application only against isolated in-memory SQLite databases; no existing database was updated.

![Relational persistence model](../_generated/diagrams/structural/diagrams/persistence-model.svg)

Source: [persistence-model.puml](diagrams/persistence-model.puml). [Unresolved questions](../decisions/index.md) require review before implementation changes.
