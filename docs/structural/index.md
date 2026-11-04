# Structural perspective

## Conceptual domain

[Smurf](xref:Smurfs.Domain.Smurf), [Pet](xref:Smurfs.Domain.Pet), and [Village](xref:Smurfs.Domain.Village) are public domain classes. This selective class model omits storage identifiers. A Smurf has an optional Pet; multiple Smurfs may share it. Village exposes a mutable collection. The modeled zero-or-one village membership expresses intent, not an enforced invariant: Smurf has no reverse village reference and can be added to several collections.

![Conceptual domain model](../_generated/diagrams/structural/diagrams/domain-model.svg)

Source: [domain-model.puml](diagrams/domain-model.puml).

## Persistence architecture

The five production projects separate domain types, provider-neutral Craft and Smurfs contracts, and reusable/Smurfs-specific EF Core implementations. Only the Smurf repository exists. One UnitOfWork owns the shared DataContext, saves it on CompleteAsync and disposes it asynchronously. The internal runtime factory and public design-time SQLite factory have different purposes. No application or composition root exists.

![Persistence architecture](../_generated/diagrams/structural/diagrams/persistence-architecture.svg)

Source: [persistence-architecture.puml](diagrams/persistence-architecture.puml). See [behavior](../behavioral/index.md) and [DataContext](xref:Smurfs.Persistence.EFCore.AppData.DataContext).

## Relational model

This separate model reflects the SQLite migration and snapshot: Smurfs and Pet tables, an optional indexed PetId foreign key, and no Village table. Village being public does not make it an EF entity. DataContext.Configure has no call site and no OnModelCreating override; conventions currently establish the model. The migration specifies no cascading delete action. No new delete policy is inferred.

![Relational persistence model](../_generated/diagrams/structural/diagrams/persistence-model.svg)

Source: [persistence-model.puml](diagrams/persistence-model.puml). [Unresolved questions](../decisions/index.md) require review before implementation changes.
