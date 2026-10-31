# Round-trip engineering conventions (reusable)

Purpose: enable AI-assisted **white-box** engineering in which implementation, diagrams, documentation, and tests remain understandable and synchronized. This file is intended to be transferable to other .NET repositories.

## Responsibilities and authority

- Codex owns the routine work of keeping documentation current: source comments, XML API documentation, PlantUML sources, relevant READMEs, and tests.
- The human maintainer remains the design authority for changed domain meaning, aggregate boundaries, transactional guarantees, public contracts, and consequential architectural trade-offs.
- Codex may implement clearly requested changes and repair demonstrable inconsistencies. For ambiguous or consequential alternatives, explain the options and request a decision rather than guessing.
- Neither PlantUML nor code is universally authoritative. Determine the task's intended direction (model-first, code-first, or reconciliation); preserve decisions across all representations.
- An absent property, association, method, or stereotype in one representation is **not** evidence it should be deleted from another.

## Routine change workflow

1. Locate the relevant source, tests, PlantUML, and existing design notes.
2. Check their consistency and note any pre-existing drift.
3. Identify changed responsibilities, types, public members, relationships, persistence assumptions, and observable behavior.
4. Implement the change with small, reviewable edits; avoid unrelated refactoring.
5. Maintain documentation **in the same task**: update PlantUML source, XML comments, and narrative guidance where needed.
6. Add or adapt appropriate tests (including contract/integration tests when behavior depends on infrastructure).
7. Execute build, tests, and available PlantUML validation; do not claim an unrun check passed.
8. Explain architectural consequences, diagrams changed, tests run, and any outstanding mismatch or decision.

## C#/.NET conventions

- Enable nullable reference types and aim for zero compiler warnings; treat warnings as errors where project configuration supports it.
- Favor clear names, small focused types, explicit contracts, and dependency inversion.
- Do not add dependencies or change public API semantics without explaining why.
- Prefer `CancellationToken` on asynchronous I/O methods and propagate it to underlying APIs.
- Never wrap synchronous operations in `Task.Run` merely to satisfy an asynchronous interface. Use genuine async I/O when available; for immediate synchronous work with a `Task` contract, return `Task.CompletedTask` or a completed result as appropriate.
- Document ownership and lifetime of disposable resources. Dispose asynchronously where supported and appropriate.
- Preserve persistence ignorance: keep framework-specific types out of domain and technology-neutral interfaces.
- Avoid assuming that a generic query expression can be translated by every persistence provider. Document supported expression semantics and test each implementation.

## XML documentation — Codex is responsible

- Document every new or changed public/protected class, interface, property, method, and constructor, following the repository's analyzer policy.
- Use `/// <summary>`, `/// <param>`, `/// <returns>`, `/// <exception>`, `/// <typeparam>`, and `/// <remarks>` when useful; use `/// <inheritdoc />` for valid inherited contracts.
- Comments must add semantic value: invariants, effects, commit boundaries, loading/tracking behavior, exceptions, null semantics, ordering, AND/OR semantics, and implementation limits. Do not merely restate identifier names.
- When behavior changes, revise existing comments; stale documentation is a defect.
- Do not fabricate guarantees (e.g., atomicity or thread safety) not supported by the implementation.

## PlantUML — Codex is responsible

- Store editable `.puml` files in `docs/architecture/`. Treat rendered artifacts, if any, as generated outputs.
- Maintain diagrams when relevant changes affect domain classes, relationships/multiplicities, interfaces, inheritance, major dependencies, or deployment boundaries.
- Keep different views distinct: **conceptual domain class diagrams**, **persistence/ER diagrams**, and **software architecture diagrams**. Do not add PK/FK annotations or EF-specific navigation concerns to a purely conceptual domain diagram.
- For conceptual C# class diagrams use this convention when accessors need to be shown: `+Name : string {get; set;}` or `+Name : string {get; private set;}`. It is a documentation convention, not a guarantee that PlantUML enforces accessor behavior.
- Include meaningful cardinalities, directionality, optional relationships, and interface generalizations; avoid speculative members.
- Prefer a few readable diagrams over a single overcrowded diagram. Update source and verify syntax when a renderer is available.
- If a change affects only internals and does not change any modeled aspect, mention that no diagram update was required.

## Testing and reviews

- Maintain unit tests for domain rules and application behavior; add integration/contract tests for repositories, provider translation, tracking behavior, commit boundaries, and error cases.
- Tests should distinguish semantics that are guaranteed by contracts from features peculiar to a provider.
- Prefer proving interchangeability of implementations with shared contract tests where practical.
- Before concluding, summarize what was verified and what remains unverified. State assumptions separately from facts.

## Persistence boundaries (technology-neutral)

- Repositories expose entity-oriented operations; a Unit of Work coordinates repositories and defines a commit boundary; factories create independent units of work.
- Technology-neutral contracts must not expose `DbContext`, `DbSet`, EF tracking entries, or provider-specific transactions.
- Define and document whether mutations merely stage changes and what `CompleteAsync` guarantees. Do not promise atomicity for all providers unless every supported implementation can deliver it.
- Define query and mutation semantics across providers (predicate combination, unsupported expressions, missing entities, duplicates, tracking, and explicit updates).
- For EF Core implementations, share one context among repositories in one Unit of Work; do not call `SaveChanges` inside individual repository operations when the contract stages changes.

## Change summary template

At the end of a task, report briefly:

- **Implementation:** classes and behavior changed.
- **Documentation:** `.puml`, XML comments, or design notes updated (or why none needed).
- **Validation:** build, tests, diagram checks, and actual outcomes.
- **Design decisions:** assumptions, alternatives, unresolved issues, and compatibility concerns.
