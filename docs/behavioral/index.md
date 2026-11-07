# Behavioral perspective

The following describes implemented behavior, verified against Repository, UnitOfWork and the existing SQLite integration tests. No reviewed sequence/activity/state model exists yet; add one when a meaningful workflow needs it.

1. A caller obtains a UnitOfWork owning a DataContext and its SmurfRepository. The runtime factory creates a context per session. The console registers the factory in its composition root.
2. CountAsync and FindAsync apply successive predicates using logical AND. An empty sequence selects all persisted rows. Results have no guaranteed ordering. Find uses configured tracking (tracking by default), with no explicit navigation Includes. A read-only result list contains mutable entities.
3. AddAsync, UpdateAsync and RemoveAsync check cancellation, then stage EF state changes without saving. They return completed tasks. Detached Update follows EF graph/state behavior and can mark mapped values modified. Tracked edits can also be saved without Update.
4. CompleteAsync delegates to SaveChangesAsync. Provider persistence, concurrency and cancellation exceptions propagate. Do not assume another provider or an HTTP-backed implementation has the same transaction guarantees.
5. DisposeAsync disposes the owned context without committing staged changes. Do not use the session concurrently.

Queries execute against database state rather than combining staged additions into query results. Plain CLR Pet/PetId assignments do not maintain consistency; an unloaded navigation may be null even when a foreign key exists.

See [IUnitOfWork](xref:Smurfs.Persistence.IUnitOfWork), [Repository](xref:Craft.Persistence.EFCore.Repository`1), the [structural perspective](../structural/index.md), and [historical coverage and gaps](../baseline-review.md). Cancellation timing, input errors, graph conflicts and failure/retry policies remain questions for review.

## Console workflow

Run these commands from the same working directory so they use the same SQLite file. If the file is missing, either Smurf verb creates the database, applies migrations and seeds sample data before running. Use `initdb` to explicitly apply migrations to an existing database.

```powershell
dotnet run --project src/Smurfs.UI.Console -- createsmurf --name "New Smurf"
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 2 --page-size 5
```

- **initdb** applies migrations and seeds sample data when the Smurfs table is empty.
- **createsmurf** sends the name to Application. The handler creates a Smurf with age zero, commits it and disposes the write session.
- **listsmurfs** opens an existing database in read-only mode. The read adapter counts rows and retrieves the requested page ordered by name and identifier. Pages start at 1, and the page size defaults to 25 and is capped at 50. An out-of-range page is empty.
