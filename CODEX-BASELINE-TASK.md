# One-time Codex task — establish perspective-based documentation baseline

## Objective

Adapt the existing Smurfs DocFX site into a perspective-based documentation website while retaining the working API reference and all existing correct design information. This is a documentation baseline task, **not** a functional refactor.

Read `AGENTS.md`, `docs/roundtrip.md`, `docs/project-context.md`, and `docs/documentation.md` first. Inspect the actual solution, DocFX configuration, source files, existing Markdown, PlantUML diagrams and `.gitignore` before editing.

## Required work

1. **Inventory and report:** Identify existing hand-authored vs generated documentation, diagram sources, DocFX `metadata`/`build.content`/`build.resource` settings, TOC paths and existing source of architectural information. Record mismatches rather than inferring missing code.
2. **Organize perspectives:** Introduce `docs/structural/`, `docs/behavioral/`, and `docs/decisions/`, with concise introductory Markdown pages. Migrate existing Markdown and `.puml` files to appropriate perspectives; preserve history where practical and repair all links. Avoid breaking the existing `docs/api/` generation.
3. **Diagrams:** Reconcile the domain and persistence PlantUML diagrams against actual C# classes/EF Core mappings. Keep conceptual class diagrams distinct from persistence/ER diagrams. Do not add speculative classes/relationships or change implementation just to match a model.
4. **Local rendering:** Establish a reproducible **local** PlantUML-to-SVG step (prefer an existing installed tool if present; otherwise create a small PowerShell script with clear prerequisites). Render images into a dedicated generated location; make DocFX include/link to them. Never send private diagrams to the public PlantUML server.
5. **Navigation:** Update DocFX main TOC and relevant per-perspective TOCs if beneficial. Ensure Structural, Behavioral, API reference and Decisions are discoverable. Retain existing API cross-references and metadata-generation functionality.
6. **Clean output discipline:** Update `.gitignore` for only the generated DocFX metadata, site and diagram assets. Keep hand-authored Markdown, `.puml`, config and scripts tracked; do not blindly ignore all SVGs.
7. **Documentation policy:** Make existing project docs point to the new perspective organization. Improve XML comments only where clearly relevant; don't perform unrelated renames or public-API changes. Add an ADR template if useful, but do not invent past decisions as if formally approved.
8. **Validation:** Execute solution build/tests when feasible, render all `.puml` files locally, build DocFX metadata and site, and check that the generated HTML includes diagrams and current public domain types (including `Village` if public in actual code). Check navigation, images and cross-reference links. Note missing tooling explicitly.

## Constraints

- No business logic changes, API renames, EF Core mapping changes or migrations unless a proven blocker is documented and approved.
- Keep the split between reusable instructions and project-specific domain context.
- Preserve existing documentation content that remains accurate; do not overwrite user's edits.
- Do not assume that a successful DocFX build validates domain consistency.
- Keep changes reviewable and avoid modifying unrelated repositories.

## Acceptance criteria

- The documentation site exposes the four perspectives with working navigation.
- Existing generated C# API reference still works.
- At least the existing structural PlantUML models are viewable as rendered diagrams in the website, locally generated from checked-in `.puml` sources.
- A single documented build sequence regenerates diagram assets and DocFX site.
- Documentation sources are versioned; generated metadata and site are ignored.
- No unintended implementation changes or EF migrations.
- Final report includes file changes, architecture discrepancies requiring human decision, verification commands/results and any outstanding limitations.

## Final report format

Summarize: (1) repository files changed, (2) model/code reconciliations and unresolved questions, (3) exact build/test/doc commands and outcomes, and (4) generated-site pages to inspect. Stop and request review before changing behavior or resolving ambiguous domain design intent.
