# Codex instructions — Smurfs

This file is the entry point for Codex working on the Smurfs solution.

## Required reading

Before making changes, read:

1. `docs/roundtrip.md` — reusable engineering and documentation workflow.
2. `docs/project-context.md` — Smurfs-specific architecture and domain constraints.
3. Relevant PlantUML diagrams under `docs/architecture/`, if present.

If one of these files is missing, say so in the task summary and do not fabricate its contents.

## Working agreement

- Act as the primary implementer **and primary documentation maintainer**. Documentation updates are part of the definition of done, not optional follow-up work.
- Preserve understandable, reviewable code. The human maintainer approves domain semantics, architectural changes, and ambiguous design choices.
- Inspect the affected code, tests, and diagrams before modifying them. Describe the intended change and its implications when it is nontrivial.
- Keep code, XML documentation, tests, and PlantUML diagrams consistent. A change is incomplete when a relevant representation is stale.
- Treat both source code and diagrams as design inputs. Do not silently overwrite one with another; reconcile conflicts and report unresolved decisions.
- Do not treat omission from a diagram as permission to delete a code member (or vice versa).
- Do not introduce EF Core, `DbContext`, or `DbSet` into persistence-independent contracts or domain classes.
- Keep edits scoped to the current repository and authorized related repositories. Never assume access to Craft just because Smurfs references it; report when a required change is outside the workspace.
- Do not use production data, secrets, credentials, or sensitive DMI material to illustrate or test the solution.

## Definition of done

For each relevant task:

1. Update implementation and relevant tests.
2. Update affected PlantUML source files (not just rendered images).
3. Add or revise XML documentation for changed public/protected APIs, including behavioral semantics and important exceptions.
4. Run available build, test, and diagram validation commands; report results and commands, or state clearly why a check could not run.
5. Summarize changed code, diagrams, documentation, outstanding questions, and any deliberate divergences.

Follow `docs/roundtrip.md` for detailed conventions. Follow `docs/project-context.md` for Smurfs-specific rules.
