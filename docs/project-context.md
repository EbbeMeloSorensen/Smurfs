# Smurfs project context (project-specific)

> This file states design intent learned so far. The checked-in code and EF Core configurations are the source of truth for **implemented** behavior. Reconcile rather than assume when they disagree.

## Purpose

Smurfs is a small C#/.NET learning/reference solution for AI-assisted roundtrip engineering. It deliberately exercises code-first domain modeling, PlantUML design models, repositories, unit of work, XML documentation and DocFX. Keep the solution understandable; do not add frameworks or complexity merely to demonstrate tooling.

## Domain intent

- `Smurf`, `Village`, and `Pet` are domain concepts.
- A `Village` can contain zero or many `Smurf` instances; each `Smurf` belongs to zero or one `Village`. This is accepted in [ADR 0001](decisions/0001-village-membership.md). Navigation goes from Village to Smurfs only; Smurf has no Village navigation. The relational model uses nullable `Smurfs.VillageId` referencing `Villages.Id`, currently represented by an EF shadow property.
- A `Smurf` has zero or one `Pet`; a `Pet` may be shared by multiple `Smurf` instances.
- Distinguish optional navigation properties from required names or other invariants. Check actual definitions and EF Core mappings before documenting exact field/property names or cascade behavior.
- Keep conceptual class models free of PK/FK stereotypes where possible. Separate persistence/ER representations may show keys and foreign keys.

## Architecture intent

- Keep domain and application contracts independent of EF Core infrastructure.
- A persistence-ignorant `IRepository<TEntity>` lives in Craft.Persistence; EF Core-specific implementations live in Craft.Persistence.EFCore or solution-specific infrastructure.
- The Smurfs persistence boundary includes repository interfaces and a unit-of-work abstraction/factory. Check the actual current interfaces and names rather than inventing method signatures.
- Preserve project boundaries among Smurfs.Domain, Smurfs.Persistence, Smurfs.Persistence.EFCore.AppData and Craft dependencies as actually present.
- EF Core is code-first. No migrations, database schema or storage behavior should be inferred purely from PlantUML.

## Domain-model changes

If a change affects classes, property types, associations, multiplicities or navigation/foreign-key relationships, inspect and reconcile `docs/structural/diagrams/domain-model.puml` and the relevant structural narrative. If persistence architecture changes, reconcile the persistence diagram. New domain types, repository specializations, entity sets and entity configurations also require reviewing direct domain dependencies in `docs/structural/diagrams/persistence-architecture.puml`, following the assembly component conventions in `roundtrip.md`. For example, a repository bound to Smurf depends on Smurf; DataContext depends on each entity type it exposes; a configuration depends on its target entity. A unit-of-work interface exposing only a repository does not thereby acquire a direct dependency on every domain type used by that repository. Inspect the current source rather than treating these examples as an exhaustive list.

Reconcile `docs/structural/diagrams/persistence-model.puml` against both the runtime EF model and the checked-in migration/snapshot, including table names, nullable and shadow foreign keys, indexes and delete behavior. Record differences explicitly. When migrations are regenerated, update any discrepancy documentation and tests that previously expected pending model changes. Behavioral diagrams should only be added for meaningful interactions or workflows, not fabricated to populate the documentation tree.
