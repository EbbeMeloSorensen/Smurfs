# Use case model

![Smurfs operator use cases](../_generated/diagrams/requirements/diagrams/use-cases.svg)

[PlantUML source](diagrams/use-cases.puml) · [Use case to requirement mapping](../_generated/traceability.md#use-case-to-requirement)

The external actor is the catalog operator. The following specifications describe candidate intent for workflows already exposed by the console. Automatic first-use setup is an observed startup behavior. The model leaves it out of the actor-goal relationships because explicit initialization is a separate operator goal. The [console workflow](../behavioral/index.md#console-workflow) realizes all three goals. The [WPF workflow](../behavioral/index.md#wpf-startup-and-paging) realizes the create form for UC-002 and adjacent-page browsing for UC-003. WPF performs setup automatically during startup.

## UC-001 Initialize local storage

- **Purpose and actor** — The catalog operator prepares local storage for use.
- **Preconditions** — The operator can write to the chosen storage location.
- **Trigger** — The operator requests initialization.
- **Main success scenario** — The operator requests setup. The system prepares storage and confirms readiness.
- **Alternatives and exceptions** — Existing storage is brought into a usable state while preserving saved Smurfs. If setup fails, the system reports the error and the operator can retry after correcting the cause.
- **Postconditions** — On success the catalog can accept later create and list requests.
- **Requirements** — See the [UC-001 mapping](../_generated/traceability.md#uc-001).

## UC-002 Create a Smurf

- **Purpose and actor** — The catalog operator records a named Smurf.
- **Preconditions** — Local storage can accept writes.
- **Trigger** — The operator supplies a proposed name and requests creation.
- **Main success scenario** — The system checks the name, saves the Smurf and confirms creation.
- **Alternatives and exceptions** — The system explains why an invalid name was rejected and does not add a Smurf. A storage failure is reported without confirming creation.
- **Postconditions** — A successfully created Smurf is available to a later request.
- **Requirements** — See the [UC-002 mapping](../_generated/traceability.md#uc-002).

## UC-003 List Smurfs

- **Purpose and actor** — The catalog operator reviews stored Smurfs.
- **Preconditions** — Local storage is readable.
- **Trigger** — The operator requests a page.
- **Main success scenario** — The system presents the requested page and total stored row count.
- **Alternatives and exceptions** — A page past the available rows is empty. Nonpositive paging inputs produce an input error and can be corrected. A storage failure is reported.
- **Postconditions** — The request does not change stored Smurfs.
- **Requirements** — See the [UC-003 mapping](../_generated/traceability.md#uc-003).

The console accepts explicit page numbers and sizes and reports the total row count. WPF starts at page 1 and lets the operator step through adjacent pages of 25 rows. WPF displays the current page and number of pages. Direct page entry, page-size selection and the total row count are available in the console. WPF prevents out-of-bound navigation through its controls. These are current client behaviors. The proposed requirements above retain their existing scope and status.
