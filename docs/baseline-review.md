> Historical report: see the [2026-11-04 baseline](baseline-2026-11-04.md). Village is now public; XML generation is enabled; models moved to [structural/diagrams](structural/index.md).

# Documentation baseline — 2026-10-31

## Scope and authority

Reviewed all six projects, their source and project references, the solution structure, migration and snapshot, tests, and the required AGENTS/engineering/project-context documents. This baseline changes documentation and tests only; runtime behavior, public signatures, package versions, visibility and domain semantics are preserved. Existing solution edits and the deletion of `docs/coming soon.txt` are preserved.

The two Craft projects are local to this solution; no separate Craft checkout was changed. No application/presentation project exists. All projects target `net10.0`, with nullable reference types and implicit usings enabled.

## Dependency inventory

| Project | Direct project dependencies | Direct package dependencies |
| --- | --- | --- |
| Smurfs.Domain | None | None |
| Craft.Persistence | None | None |
| Craft.Persistence.EFCore | Craft.Persistence | Microsoft.EntityFrameworkCore 10.0.14 |
| Smurfs.Persistence | Craft.Persistence; Smurfs.Domain | None |
| Smurfs.Persistence.EFCore.AppData | Craft.Persistence.EFCore; Smurfs.Persistence | EF Core, Design, Relational, Sqlite 10.0.14; Design is private tooling dependency |
| Smurf.Persistence.EFCore.AppData.UnitTest | Smurfs.Persistence.EFCore.AppData | coverlet.collector 6.0.4; Microsoft.NET.Test.Sdk 17.14.1; xunit 2.9.3; xunit.runner.visualstudio 3.1.4 |

The domain and neutral contracts contain no EF references. Reusable EF code does not depend on Smurfs. Provider-specific factory types stay inside the implementation. No dependency upgrade or vulnerability audit is included in this baseline.

## Verified behavior

- `IRepository<TEntity>` combines read and write interfaces. Count and Find apply successive `Where` calls (AND); empty predicates select all persisted rows. Results have no ordering guarantee. Null predicate sequences and unsupported expressions have no custom validation policy.
- Find follows the supplied context's tracking configuration; default reads track. It neither calls `AsNoTracking` nor explicitly includes navigations. The list is read-only, but its entities are mutable. Tracked edits can be saved without Update.
- Add, Update and Remove check cancellation synchronously, call EF's corresponding entity-set operation, and return a completed task without saving. EF graph/state behavior applies, including detached Update marking mapped values modified. Queries operate against database state rather than merging staged changes into results.
- `UnitOfWork` creates one Smurf repository with its owned context. Complete delegates to `SaveChangesAsync`; Dispose delegates to asynchronous context disposal without saving. Only Smurfs is exposed; no Pet/Village repositories exist. Commit guarantees remain provider-specific.
- The internal runtime factory requests a context from `IDbContextFactory<DataContext>` per unit of work. No runtime composition root or public registration extension exists in this solution. The separate design-time factory uses SQLite `smurfs.db` relative to the working directory and ignores tooling arguments.
- `DataContext` exposes Smurfs. Pet is discovered through its navigation. Village is absent from the model. Static `Configure` is not wired into model creation, so the explicit configuration is inactive. Its current key configuration is equivalent to convention; the snapshot and migration match the active model.
- Domain names use required initialization without runtime non-empty validation; Age has no range checks. Pet can be shared. Plain CLR assignments do not maintain Pet/PetId consistency. Village is internal; its collection does not enforce a single village per Smurf.

## Documentation changes

- Conceptual diagram now includes confirmed names/age, internal Village visibility and a prominent note distinguishing intended membership multiplicity from unenforced source behavior. Storage identifiers remain omitted by design.
- Architecture diagram now includes read/write interface inheritance, Unit of Work members, async disposal, the runtime context-factory dependency, design-time factory and inactive configuration helper.
- Added a separate persistence diagram for current tables, keys, nullable indexed PetId and migration delete-action facts.
- Added XML documentation throughout handwritten domain, contracts and implementation APIs, including protected context/constructor, staging, tracking, ownership, expression limitations and cancellation parameters. Tests also document their behavioral intent. Generated migration sources were preserved.
- Added verified observations to project context; the working agreement in AGENTS.md and roundtrip.md already establishes ongoing synchronization responsibility.

## Findings and recommended decisions

1. **Inactive model-configuration hook (medium):** future configuration changes could silently have no effect. Recommend wiring configuration through `OnModelCreating`, with a regression test demonstrating that the hook executes. The current mapping is equivalent to conventions, so this baseline documents the issue without changing the model-building mechanism.
2. **Village concept differs from enforced structure (medium):** the intended optional single membership cannot be guaranteed by the mutable collection. Village also cannot be consumed outside the domain assembly or persisted. Decide its intended lifecycle, visibility and relationship semantics before adding reverse navigation, mapping, repositories or exclusivity rules. No new aggregate or cascade policy is inferred.
3. **Tracking versus provider portability (medium):** default tracked reads allow implicit updates, while detached Update can overwrite more values than intended and affect reachable graphs. Decide whether to retain tracked reads or adopt no-tracking reads with explicit updates, and define concurrency/partial-update expectations first. This baseline preserves and tests current tracking.
4. **Runtime factory accessibility/composition (medium):** internal `UnitOfWorkFactory` has no public registration path. Recommend a deliberate public registration API or visibility choice when an external consumer is introduced; do not widen access merely for testing.
5. **Contract edge cases (medium):** null inputs, duplicate keys, missing updates/deletes, graph attachment conflicts, cancellation timing, supported predicates and provider transaction guarantees need explicit policies before interchangeable implementations can be claimed. The factory constructor also lacks a null guard, unlike UnitOfWork and Repository. Recommend consistent guards as a separate small behavior change.
6. **Domain validation (decision required):** source enforces neither valid ages/non-empty names nor Pet/PetId consistency. Agree on actual domain rules before introducing validation; required initialization alone is not validation.
7. **Documentation enforcement (low):** XML output, missing-comment diagnostics and diagram validation are not enabled by project/CI policy. Recommend an agreed CI check for production APIs and `.puml` syntax. No analyzer policy or project renaming was imposed.

## Test baseline and remaining gaps

Initially there was one passing SQLite integration test despite the project's UnitTest name. Added ten test cases (seven facts plus a three-case theory; the original fact makes eleven):

- AND filters, empty filters, count/find agreement and no matches;
- default tracking and persistence of tracked edits;
- detached update staging and Unit of Work commit;
- insert/delete staging, verified with fresh contexts;
- disposal without commit and ownership of context disposal;
- pre-cancelled Add/Update/Remove leaving the tracker empty;
- snapshot consistency and optional many-to-one Pet mapping;
- applying the actual migration to a fresh SQLite database and saving/querying a Pet association.

Remaining prioritized coverage:

| Priority | Gap | Recommended verification |
| --- | --- | --- |
| High | Runtime factory independence | Exercise two factory-created sessions and disposal isolation once a supported registration/test access seam is agreed. Current tests construct UnitOfWork directly. |
| High | Failed commits and atomicity | Induce a relational constraint failure across multiple staged operations and verify persisted rows from a fresh context; test retry/disposal expectations after agreeing the contract. |
| High | Detached update hazards | Test graphs, two instances with the same key, generated/unset keys and overwritten fields; define missing-row/concurrency semantics first. |
| Medium | Cancellation | Add query/commit cancellation and non-pre-cancelled execution cases; current cancellation tests cover only already-cancelled mutations. |
| Medium | Input/provider errors | Test null arguments, null predicates, untranslatable expressions, duplicates and missing deletes after defining desired contract semantics. |
| Medium | Shared pets and deletion | Persist multiple Smurfs sharing a Pet; test explicit loading and agreed deletion behavior. Current model test checks cardinality only. |
| Medium | Domain unit tests | Characterize identifier initialization and collection defaults; add invariant tests when domain rules are approved. No domain unit-test suite currently exists. |
| Future | Provider interchangeability | Extract shared contract tests when a second implementation exists; SQLite results do not prove HTTP/in-memory equivalence. |

## Validation

Performed first in an isolated source copy because the repository is outside the initially writable workspace; final repository verification is recorded below after application.

- Original `dotnet test Smurfs.sln --no-restore --verbosity minimal`: 1 passed.
- Updated `dotnet test Smurfs.sln --no-restore --verbosity minimal`: 11 passed, 0 failed/skipped.
- `dotnet build Smurfs.sln --no-restore --verbosity minimal -p:GenerateDocumentationFile=true -warnaserror`: passed with 0 warnings and 0 errors after resolving missing-comment warnings in test APIs.
- Local PlantUML (`jebbs.plantuml-2.18.1/plantuml.jar`) `-checkonly` on all three diagram sources: exit 0. This is syntax validation; visual render verification is separate.

No production data, credentials or external database were used. No architecture or domain-model change has been implemented. The recommendations above remain pending design approval.

### Final verification in the target repository

After applying the reviewed files to `C:\Git\GitHub\Smurfs`:

- `dotnet test Smurfs.sln --no-restore --verbosity minimal -p:GenerateDocumentationFile=true -warnaserror`: build and all 11 tests passed (0 failures/skips).
- Local PlantUML `-checkonly` on all three target diagram sources: exit 0.
- `git diff --check`: exit 0.
- Existing solution edits, untracked guidance files and the prior documentation deletion were preserved. No commit was created.
