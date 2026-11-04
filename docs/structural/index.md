# Structural perspective

## Conceptual domain

[Smurf](xref:Smurfs.Domain.Smurf), [Pet](xref:Smurfs.Domain.Pet), and [Village](xref:Smurfs.Domain.Village) are public domain classes. This selective class model omits storage identifiers. A Smurf has an optional Pet; multiple Smurfs may share it. Village exposes a mutable collection. The modeled zero-or-one village membership expresses intent, not an enforced invariant: plain CLR collections permit a Smurf in several villages. EF tracking represents membership with a single optional shadow VillageId foreign key; Smurf has no reverse village reference.

![Conceptual domain model](../_generated/diagrams/structural/diagrams/domain-model.svg)

Source: [domain-model.puml](diagrams/domain-model.puml).

## Persistence architecture

The five production projects separate domain types, provider-neutral Craft and Smurfs contracts, and reusable/Smurfs-specific EF Core implementations. Only the Smurf repository exists. One UnitOfWork owns the shared DataContext, saves it on CompleteAsync and disposes it asynchronously. The internal runtime factory and public design-time SQLite factory have different purposes. No application or composition root exists.

![Persistence architecture](../_generated/diagrams/structural/diagrams/persistence-architecture.svg)

Source: [persistence-architecture.puml](diagrams/persistence-architecture.puml). See [behavior](../behavioral/index.md) and [DataContext](xref:Smurfs.Persistence.EFCore.AppData.DataContext).

## Relational model

This separate model reflects the current EF runtime model: Smurfs, Villages and Pets tables. DataContext exposes all three entity sets. Smurf has optional indexed PetId and shadow VillageId foreign keys; each village and pet can be associated with multiple Smurfs. VillageId is nullable and maintained by EF, with no corresponding C# property. Both optional relationships use ClientSetNull by convention. DataContext.Configure has no call site and no OnModelCreating override; conventions currently establish the model.

**Needs reconciliation:** the checked-in SQLite migration and snapshot still define Smurfs and Pet (singular), with no Villages table or VillageId column. Adding DbSets changes the runtime model, not an existing database. A reviewed migration is still needed, including the Pet-to-Pets table rename and village membership schema. No migration or database update was performed during this reconciliation.

![Relational persistence model](../_generated/diagrams/structural/diagrams/persistence-model.svg)

Source: [persistence-model.puml](diagrams/persistence-model.puml). [Unresolved questions](../decisions/index.md) require review before implementation changes.
