# ADR 0002: Explicit Pet mapping during model creation

Status: Accepted

Date: 2026-11-04

## Context

The project owner added an explicit optional Smurf-to-Pet relationship with DeleteBehavior.Restrict and then wired OnModelCreating. Configuration belongs alongside the existing Smurf key mapping rather than being split across DataContext and SmurfConfiguration.

## Decision

DataContext.OnModelCreating calls the base implementation and the public static Configure helper. The helper applies SmurfConfiguration, which owns the Smurf primary key and optional, non-unique PetId relationship. Pet has no reverse collection. Preserve the owner's Restrict choice; village membership remains convention-based with ClientSetNull.

## Consequences and validation

Restrict creates a non-cascading database foreign key with ON DELETE RESTRICT. Deleting a Smurf does not delete its Pet. Deleting a Pet while untracked Smurfs still reference it fails at the database. For tracked Smurfs, EF can clear their nullable PetId values before deleting the Pet. This is not a domain rule forbidding deletion of every referenced Pet, and no such rule is introduced.

Relational tests exercise both tracked and untracked Pet deletion against a migrated in-memory SQLite database. Model metadata verifies Restrict, proving the explicit configuration is active; snapshot consistency verifies the migration agrees with it. The initial migration is regenerated to retain the single-baseline approach used during development. No existing database is updated or deleted.

## Review

The project owner approved wiring model creation, moving the relationship into SmurfConfiguration, and reconciling tests and migration in this conversation on 2026-11-04. Restrict preserves the policy explicitly chosen in their code; a stronger domain-level deletion prohibition would require a separate decision.
