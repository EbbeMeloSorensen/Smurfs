# Verification and validation

The plan covers console and desktop workflows, JWT API access, WebClient create/detail/edit/delete flows, filtering/sorting, domain rules, and the PostgreSQL/SQLite database providers and JSON file adapter. The [traceability matrix](../_generated/traceability.md) records evidence links and limitations.

| Level | Scope and current evidence |
| --- | --- |
| Unit | Isolated rule and handler checks are appropriate. There is no separate C# unit-test project; React component/store tests are listed below. |
| Integration | The existing xUnit project exercises EF Core with real, in-memory SQLite and isolated temporary JSON files. Its historical project name contains `UnitTest`. |
| Component/client | The Jest/React Testing Library suite tests forms, detail rendering, store filtering/sorting, stale requests, and canonical/legacy routes. API requests are mocked. |
| System | Run clients and the API against disposable stores. Live browser-to-API and desktop interaction remain manual. |
| Acceptance | A human runs the [manual procedures](acceptance.md) against the approved baseline. Proposed cases may be run for review and marked provisional. |

## Execution plan

- **Entry** — Identify the commit and requirement status, build the solution with the selected SDK, and prepare isolated data. Confirm the acceptance scope with the human reviewer.
- **Environments and data** — Run `dotnet test Smurfs.sln` for integration. Tests open and dispose their own in-memory SQLite connections. For the built clients use a disposable PostgreSQL instance and a fresh working directory and [the acceptance setup](acceptance.md#isolated-setup).
- **Techniques** — Use automated assertions for persisted state and schema, document/model inspection for engineering controls, and client boundary and error scenarios for system and acceptance evidence.
- **Responsibilities** — The contributor maintains links and runs automated checks. The tester records actual outcomes. The human engineer approves requirements, dispositions deviations and decides acceptance.
- **Exit** — Record build and automated test results, traceability diagnostics and the status of each applicable manual case. Review all failures, blocks and uncovered requirements before acceptance. Explicitly record any agreed deferral.
- **Defects and deviations** — Record the requirement and case IDs, version, environment, expected and observed results, and reproduction steps. Link the defect or approval for a deviation. Rerun the affected checks after a fix.
- **Reporting** — Use the [execution record template](execution-record.md) for manual runs. Automated results are separate evidence. No manual result is implied by the build.

## Existing automated suite

The tests in `tests/Smurf.Persistence.EFCore.AppData.UnitTest` exercise repository predicates, tracking, staged writes, disposal, cancellation, fresh-schema migration, snapshot agreement, Village and Pet round trips, and tracked and untracked Pet deletion. Relevant methods are indexed in the matrix. The SQLite tests use `SqliteDataContext` and its matching migration set. Provider migration tests check isolated discovery and snapshot agreement for both SQLite and PostgreSQL without opening a PostgreSQL connection. Identity provider tests verify its migration discovery and snapshots, account creation and password checks through the shared context registration, and repeatable migration of Identity and AppData in one SQLite database. Live PostgreSQL migration application remains a separate integration check. Initializer reruns are covered on SQLite. These tests do not exercise console parsing, WPF dialogs/navigation, live PostgreSQL migration application, or formal user acceptance. The create-handler integration test checks catalog rejection at name boundaries, an additional draft rule, and a successful SQLite write. A referenced integration test can establish only part of a product requirement.


## WebClient tests

Run npm ci in src/Smurfs.WebClient when dependencies need restoring, then npm test -- --watchAll=false --runInBand. The client uses mocked API responses for create/edit payloads and failures, nullable detail display, filter resets, stale-response suppression, server ordering, and route redirects. These checks do not establish that a deployed browser and API interoperate; AT-009 and AT-010 cover that manual review.

Application integration tests also cover Details, Edit, Delete, filtered totals and paging, creation-time ordering with ties, fixed UTC seed times, and field projection. The traceability matrix links these tests to the corresponding proposed requirements. The complete backend suite should pass against the checked-in migrations. Migration-file presence and snapshot agreement do not prove live PostgreSQL upgrade success.

## File persistence tests

`FilePersistenceTests` uses unique temporary directories and independent factories/sessions. It verifies repeatable/concurrent initialization, restart round trips of IDs and scalar/null values, tracked and detached edits, disposal without commit, shared pets and preserved village membership, existing Create/Edit/Delete handlers and DI registration, AND predicates, filtered/sorted paging, stale-write rejection, cancellation, failed serialization cleanup, and malformed/unsupported file preservation. These checks use the local filesystem; they do not certify network filesystems or power-loss durability. Console smoke checks should run separate `initdb`, `createsmurf`, and `listsmurfs` processes with `Database__Provider=File` and a disposable absolute `FileStore__Path`. Formal human acceptance remains separate.

## API client and command tests

ApiClientTests verifies HTTP login, bearer headers, creation, reads, metadata, failures, cancellation, and no automatic replay. ApiConsoleTests verifies saved login reused by independent command instances, one operation per invocation, original create/list options and output, missing/rejected credentials without prompts, environment override precedence, explicit stdin login, endpoint-scoped logout, and uncertain-create failure without list reload. A Windows-only native test creates a unique credential entry, reads it through another store instance, replaces it, and always removes it. Portable command tests use an in-memory ITokenStore. Automated checks do not constitute formal human acceptance.

Live-process smoke checks should run a disposable API/database, authenticate once, invoke list/create as separate processes, verify credential reuse/logout and environment overrides, and check failure exit codes. Use a unique API port and remove its test credential in a finally block. [AT-011](acceptance.md#at-011-api-console-login-and-paging) and [AT-012](acceptance.md#at-012-api-console-name-only-creation) cover human review. Dataset import remains outside this milestone.
