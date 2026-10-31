# Smurfs project context (project-specific)

## Purpose

Smurfs is a deliberately small reference solution for exploring round-trip engineering with Codex. Use it as a high-quality example of understandable C#/.NET design rather than maximizing feature count. The engineering rules live in `docs/roundtrip.md`; keep domain facts and solution-specific architectural choices here.

## Organization and boundaries

- Prefer physical folders `src/`, `tests/`, and `docs/`; align Visual Studio solution folders where practical. Inspect the existing solution rather than reorganizing it without being asked.
- `Smurfs.Domain`: domain types such as `Smurf`, `Village`, and `Pet`; no EF Core or persistence implementation dependency.
- `Craft.Persistence`: reusable `IRepository<TEntity>` technology-neutral contract (currently included under this solution’s `src/`).
- `Craft.Persistence.EFCore`: reusable EF Core `Repository<TEntity> : IRepository<TEntity>` implementation.
- `Smurfs.Persistence`: Smurfs-specific `ISmurfRepository`, `IPetRepository`, `IVillageRepository`, `IUnitOfWork`, `IUnitOfWorkFactory` contracts as actually present. Do not assume all three repositories are implemented yet.
- `Smurfs.Persistence.EFCore.AppData`: EF Core `DataContext`, `SmurfRepository`, `UnitOfWork`, and `UnitOfWorkFactory` (and other repositories if/when implemented).
- Application/presentation components may be added or retained according to the actual repository state; do not invent projects or introduce an API solely to generate EF Core migrations.

## Domain â€” known conceptual rules

- A `Village` can contain zero or more `Smurf` instances.
- A `Smurf` belongs to zero or one `Village`.
- A `Smurf` has zero or one `Pet`.
- A `Pet` may be shared by zero or more `Smurf` instances.
- Do not infer domain properties, invariants, aggregate boundaries, deletion behavior, or cascade rules that have not been agreed.
- The conceptual domain diagram is `docs/architecture/domain-model.puml`. It intentionally omits primary keys, foreign keys, EF Core annotations, and other storage concerns. Create a separate ER/persistence diagram if needed.
- Domain structure should be synchronized with source when verified. A diagram is not a license to rewrite pre-existing code that differs from the current conceptual sketch; report the drift.

## Persistence design decisions

- `IRepository<TEntity>` is deliberately provider-independent. The intended implementation set may include EF Core, an in-memory repository, and an HTTP/API-backed repository. Do not replace this abstraction with `DbSet<TEntity>` or `DbContext` in consumer-facing contracts.
- `ISmurfRepository` specializes the generic repository; its concrete EF Core implementation can remain a minimal subclass until it needs domain-specific queries.
- `IUnitOfWork` exposes repositories; `IUnitOfWorkFactory` creates distinct units of work. The preferred factory design uses `IDbContextFactory<DataContext>` **inside the EF Core implementation**, not in the neutral factory contract.
- All EF Core repositories within one `UnitOfWork` share its `DataContext`.
- `CompleteAsync(CancellationToken)` is the commit point and delegates to `DataContext.SaveChangesAsync`. `DisposeAsync` delegates to the context's async disposal.
- Repository mutations register changes; they do not invoke `SaveChanges` individually. EF Core `Add`, `Update`, and `Remove` can complete synchronously under an async interface; never use `Task.Run` merely to wrap them.
- Multi-predicate filters are combined using **AND**, by composing successive `Where` operations on `IQueryable<TEntity>` before execution, for the EF Core implementation. Document that in the neutral contract as well.
- Read queries and update semantics require care: explicit updates are preferred for provider-neutral behavior. Confirm whether read queries use `AsNoTracking()` and document the final policy. Do not silently change tracking because detached-entity `Update` has important consequences.
- Do not promise that an API-backed Unit of Work is atomic unless the target API supports suitable transaction semantics. Document provider-specific limitations rather than weakening existing guarantees silently.

## Modeling and docs to maintain

- `docs/architecture/domain-model.puml`: conceptual Smurf/Village/Pet relationships and confirmed domain structure.
- `docs/architecture/persistence-architecture.puml`: interfaces and implementation boundaries, with arrows reflecting actual dependencies.
- XML documentation in the source: Codex maintains public contract semantics, especially asynchronous operations, query filters, staging versus commit, tracking, and disposal.
- When new domain properties/associations are agreed, update code, tests, and diagrams together. When a persistence detail changes, update the architecture diagram; update an ER diagram separately if one exists.

## Initial verification tasks for Codex

1. Inspect actual project names, source properties, and repository signatures; correct diagrams or notes if they differ, but **do not silently change domain meaning**.
2. Confirm whether the current `IUnitOfWork` exposes only `Smurfs` or also `Pets` and `Villages`; update the architecture diagram to match implementation.
3. Verify whether repository `FindAsync` uses no-tracking queries and whether `UpdateAsync` contract tests cover detached instances.
4. Run the existing build and tests, note gaps, and propose integration tests for EF Core repositories and Unit of Work.
5. Maintain this file when a project-specific decision is explicitly made; do not expand it with assumptions.


## Verified baseline — 2026-10-31

These are observations of the current code, not additional design decisions:

- `IRepository<TEntity>` inherits `IReadRepository<TEntity>` and `IWriteRepository<TEntity>`; both Craft projects are included locally in this solution.
- Only `ISmurfRepository` exists. `IUnitOfWork` exposes only `Smurfs`; no Pet/Village repository contracts or implementations exist.
- `FindAsync` uses the context's configured tracking policy, which is tracking by default. It does not call `AsNoTracking` or explicitly include navigations. Explicit updates remain the portable usage recommendation, not a requirement enforced by EF reads.
- `Village` is internal and has a mutable `Smurfs` collection. There is no reverse village property on `Smurf`, no exclusive-membership enforcement, and no Village entity in the EF model.
- `DataContext.Configure` has no call site and there is no `OnModelCreating` override; the active model uses conventions. The migration snapshot currently matches it.
- The runtime `UnitOfWorkFactory` is internal and uses `IDbContextFactory<DataContext>`. The public `DataContextFactory` is a separate design-time SQLite factory.
- See [baseline review](baseline-review.md) for test coverage, remaining gaps and recommendations requiring a design decision.
