# ADR 0001: Optional single-village membership without reverse navigation

Status: Accepted

Date: 2026-11-04

## Context

Village exposes a collection of Smurfs. DataContext exposes Villages, Smurfs and Pets. The current EF runtime model discovers an optional one-to-many village relationship through a shadow VillageId property on Smurf. At approval, the migration snapshot still predated this mapping; the regenerated initial migration now includes it.

## Options

- Optional one-to-many membership with navigation from Village to Smurfs only: matches the agreed domain and existing EF runtime model.
- Required membership: would prevent a Smurf from having no village.
- Many-to-many membership: would permit simultaneous membership in multiple villages.
- Reverse Village navigation on Smurf: would expose a traversal the project owner does not want.

## Decision

A Village contains zero or many Smurfs. A Smurf belongs to zero or one Village and may never belong to multiple villages simultaneously. Navigation is from Village to its Smurfs only; Smurf has no Village navigation property.

In the relational model, Smurfs.VillageId is a nullable foreign key referencing the primary key Villages.Id. NULL represents no village. The foreign key is not unique, so multiple Smurfs may reference the same Village. The single foreign-key value permits at most one village per Smurf row.

Retain the existing shadow VillageId mapping; a relational foreign key does not require a C# property or reverse navigation on Smurf.

## Consequences and validation

The current EF runtime mapping and relational diagram agree with this decision. Existing SQLite tests verify nullable shadow VillageId, optional non-unique membership, and a round trip through Village.Smurfs.

Subsequent implementation on 2026-11-04: regenerated initial migration `20261104164331_InitialMigration` and its snapshot now create Smurfs, Pets and Villages, including nullable indexed VillageId referencing Villages.Id. The old initial migration was replaced, so no Pet-to-Pets rename operation is needed in the new baseline. The baseline was subsequently regenerated again for the explicit Pet delete policy in [ADR 0002](0002-pet-configuration.md); village mapping is unchanged. Migration application is validated against a fresh in-memory SQLite database; no existing database was updated.

Plain mutable Village.Smurfs collections can still contain conflicting memberships before EF tracking. This is a known implementation limitation, not permission for multiple membership. How domain objects should prevent that conflict remains an open implementation question. No domain API, reassignment workflow or delete policy is introduced here.

## Review

Approved by the project owner in this conversation on 2026-11-04: membership is optional, limited to one village, has no reverse navigation from Smurf, and uses a foreign key from Smurf to Village in the ER model.
