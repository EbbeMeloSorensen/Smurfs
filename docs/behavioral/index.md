# Behavioral perspective

- [Console workflows](console.md) shows one sequence per verb.
- [WPF workflows](wpf.md) separates application lifetime from operator actions.

## Shared persistence behavior

The generic repository and unit-of-work behavior below is covered by the SQLite integration tests. Both clients use a separate read service for listing.

1. A caller obtains a UnitOfWork owning a DataContext and its SmurfRepository. The runtime factory creates a context per session. The console registers the factory in its composition root.
2. CountAsync and FindAsync apply successive predicates using logical AND. An empty sequence selects all persisted rows. Results have no guaranteed ordering. Find uses configured tracking (tracking by default), with no explicit navigation Includes. A read-only result list contains mutable entities.
3. AddAsync, UpdateAsync and RemoveAsync check cancellation, then stage EF state changes without saving. They return completed tasks. Detached Update follows EF graph/state behavior and can mark mapped values modified. Tracked edits can also be saved without Update.
4. CompleteAsync delegates to SaveChangesAsync. Provider persistence, concurrency and cancellation exceptions propagate. Do not assume another provider or an HTTP-backed implementation has the same transaction guarantees.
5. DisposeAsync disposes the owned context without committing staged changes. Do not use the session concurrently.

Queries execute against database state rather than combining staged additions into query results. Plain CLR Pet/PetId assignments do not maintain consistency; an unloaded navigation may be null even when a foreign key exists.

See [IUnitOfWork](xref:Smurfs.Persistence.IUnitOfWork), [Repository](xref:Craft.Persistence.EFCore.Repository`1), the [structural perspective](../structural/index.md), and [historical coverage and gaps](../baseline-review.md). Cancellation timing, input errors, graph conflicts and failure/retry policies remain questions for review.

## Console workflow

See the [Console workflows](console.md) for [initialization](console.md#initdb), [creation](console.md#createsmurf) and [listing](console.md#listsmurfs).

## WPF startup and paging

See the [WPF workflows](wpf.md) for [application lifetime](wpf.md#application-lifetime) and [browsing](wpf.md#browse-smurfs).

### Create a Smurf in WPF

See the [WPF create workflow](wpf.md#create-a-smurf).
