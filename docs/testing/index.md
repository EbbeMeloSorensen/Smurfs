# Verification and validation

The plan covers local console workflows, WPF paging, Smurf domain rules and app-data persistence. The separate Identity adapter is outside the current acceptance scope. The [traceability matrix](../_generated/traceability.md) records evidence links and limitations.

| Level | Scope and current evidence |
| --- | --- |
| Unit | Isolated rule and handler checks are appropriate. The current solution has no dedicated isolated unit suite. |
| Integration | The existing xUnit project exercises EF Core with real, in-memory SQLite. Its historical project name contains `UnitTest`. |
| System | Run the console or WPF client against a disposable working directory. No automated client system suite is currently checked in. |
| Acceptance | A human runs the [manual procedures](acceptance.md) against the approved baseline. Proposed cases may be run for review and marked provisional. |

## Execution plan

- **Entry** — Identify the commit and requirement status, build the solution with the selected SDK, and prepare isolated data. Confirm the acceptance scope with the human reviewer.
- **Environments and data** — Run `dotnet test Smurfs.sln` for integration. Tests open and dispose their own in-memory SQLite connections. For the built clients use a fresh disposable working directory and [the acceptance setup](acceptance.md#isolated-setup).
- **Techniques** — Use automated assertions for persisted state and schema, document/model inspection for engineering controls, and client boundary and error scenarios for system and acceptance evidence.
- **Responsibilities** — The contributor maintains links and runs automated checks. The tester records actual outcomes. The human engineer approves requirements, dispositions deviations and decides acceptance.
- **Exit** — Record build and automated test results, traceability diagnostics and the status of each applicable manual case. Review all failures, blocks and uncovered requirements before acceptance. Explicitly record any agreed deferral.
- **Defects and deviations** — Record the requirement and case IDs, version, environment, expected and observed results, and reproduction steps. Link the defect or approval for a deviation. Rerun the affected checks after a fix.
- **Reporting** — Use the [execution record template](execution-record.md) for manual runs. Automated results are separate evidence. No manual result is implied by the build.

## Existing automated suite

The tests in `tests/Smurf.Persistence.EFCore.AppData.UnitTest` exercise repository predicates, tracking, staged writes, disposal, cancellation, fresh-schema migration, snapshot agreement, Village and Pet round trips, and tracked and untracked Pet deletion. Relevant methods are indexed in the matrix. These tests do not exercise console parsing, command dispatch, name-policy boundaries, initializer reruns, WPF navigation, or user acceptance. A referenced integration test can establish only part of a product requirement.
