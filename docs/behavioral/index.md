# Behavioral perspective

The generic repository and unit-of-work behavior below is covered by the SQLite integration tests. The console list use case uses the separate read service described in the console workflow.

1. A caller obtains a UnitOfWork owning a DataContext and its SmurfRepository. The runtime factory creates a context per session. The console registers the factory in its composition root.
2. CountAsync and FindAsync apply successive predicates using logical AND. An empty sequence selects all persisted rows. Results have no guaranteed ordering. Find uses configured tracking (tracking by default), with no explicit navigation Includes. A read-only result list contains mutable entities.
3. AddAsync, UpdateAsync and RemoveAsync check cancellation, then stage EF state changes without saving. They return completed tasks. Detached Update follows EF graph/state behavior and can mark mapped values modified. Tracked edits can also be saved without Update.
4. CompleteAsync delegates to SaveChangesAsync. Provider persistence, concurrency and cancellation exceptions propagate. Do not assume another provider or an HTTP-backed implementation has the same transaction guarantees.
5. DisposeAsync disposes the owned context without committing staged changes. Do not use the session concurrently.

Queries execute against database state rather than combining staged additions into query results. Plain CLR Pet/PetId assignments do not maintain consistency; an unloaded navigation may be null even when a foreign key exists.

See [IUnitOfWork](xref:Smurfs.Persistence.IUnitOfWork), [Repository](xref:Craft.Persistence.EFCore.Repository`1), the [structural perspective](../structural/index.md), and [historical coverage and gaps](../baseline-review.md). Cancellation timing, input errors, graph conflicts and failure/retry policies remain questions for review.

## Console workflow

![Console setup and use-case dispatch](../_generated/diagrams/behavioral/diagrams/console-workflow.svg)

The [use case model](../requirements/use-cases.md) specifies the proposed operator goals. The sequence below shows their current implementation, including first-use setup.

The [sequence source](diagrams/console-workflow.puml) shows startup and use-case dispatch. The host disposes its service scope after the command completes.

Run these commands from the same working directory so they use the same SQLite file. If the file is missing, either Smurf verb creates the database, applies migrations and seeds sample data before running. Use `initdb` to explicitly apply migrations to an existing database.

```powershell
dotnet run --project src/Smurfs.UI.Console -- createsmurf --name "New Smurf"
dotnet run --project src/Smurfs.UI.Console -- listsmurfs --page 2 --page-size 5
```

- **initdb** applies outstanding migrations, then seeds sample data if the Smurfs table is empty.
- **createsmurf** validates the proposed name before constructing a Smurf with age zero. The handler runs the catalog rules before opening a write session, then commits and disposes the session.
- **listsmurfs** opens an existing database in read-only mode. The read adapter counts rows and retrieves the requested page ordered by name and identifier. Pages start at 1, and the page size defaults to 25 and is capped at 50. An out-of-range page is empty. The identifier breaks ties between equal names. Count and page retrieval run as separate queries, so concurrent writes can change page contents or totals.

Names must contain non-whitespace text and be at most 20 characters. The console, the domain property and the catalog use the same policy. The console writes invalid-name messages to standard error. If the catalog returns several violations, the handler reports the first. The list handler returns a failure for nonpositive paging values. The console returns exit code 1 for use-case failures and caught runtime exceptions. If setup fails after the file is created, run `initdb` to retry setup on the existing file. The console filters the MediatR license log category.
