# Use case model

![Smurfs operator use cases](../_generated/diagrams/requirements/diagrams/use-cases.svg)

[PlantUML source](diagrams/use-cases.puml) · [Use case to requirement mapping](../_generated/traceability.md#use-case-to-requirement)

The catalog operator is the external actor. These specifications describe the current catalog goals for requirements review. Console initialization is explicit; desktop and API startup also initialize their configured stores. Console and WPF create named Smurfs, while the WebClient supplies the richer create fields. The HTTP API and WebClient implement detail, edit, and delete workflows. Existing approval states remain in the [traceability matrix](../_generated/traceability.md).

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
- **Preconditions** — Storage can accept writes locally or through an authenticated HTTP connection.
- **Trigger** — The operator supplies a proposed name and requests creation.
- **Main success scenario** — The system checks the name, saves the Smurf and confirms creation.
- **Alternatives and exceptions** — The system explains why an invalid name was rejected and does not add a Smurf. A storage failure is reported without confirming creation.
- **Postconditions** — A successfully created Smurf is available to a later request.
- **Requirements** — See the [UC-002 mapping](../_generated/traceability.md#uc-002).

## UC-003 List Smurfs

- **Purpose and actor** — The catalog operator reviews stored Smurfs.
- **Preconditions** — Storage is readable locally or through an authenticated HTTP connection.
- **Trigger** — The operator requests a page.
- **Main success scenario** — The system applies selected name/infection filters and ordering, then presents the requested page and matching row count where the client exposes it.
- **Alternatives and exceptions** — A page past the available rows is empty. Nonpositive paging inputs produce an input error and can be corrected. A storage failure is reported.
- **Postconditions** — The request does not change stored Smurfs.
- **Requirements** — See the [UC-003 mapping](../_generated/traceability.md#uc-003).

The console accepts explicit page numbers and sizes and reports the total row count. WPF starts at page 1 and lets the operator step through adjacent pages of 25 rows. WPF displays the current page and number of pages. Direct page entry, page-size selection and the total row count are available in the console. WPF prevents out-of-bound navigation through its controls. These are current client behaviors. The proposed requirements above retain their existing scope and status.

Avalonia and WinForms use the shared desktop paging view model. The [HTTP API](../behavioral/http-api.md#list-smurfs) realizes UC-003 through an authenticated GET and returns totals in the pagination header.

## UC-004 Inspect a Smurf

- **Actor and goal** — The operator opens a selected Smurf's details.
- **Preconditions** — Storage is readable; HTTP access is authenticated.
- **Scenario** — Select a Smurf or open its detail URL. The system returns its current scalar values and represents unknown values explicitly.
- **Exceptions** — A missing identifier is reported as not found. Retrieval failures are reported.
- **Postconditions** — Stored data is unchanged.
- **Requirements** — See the [UC-004 mapping](../_generated/traceability.md#uc-004).

## UC-005 Edit a Smurf

- **Actor and goal** — The operator corrects an existing Smurf's details.
- **Preconditions** — The Smurf exists and the HTTP caller is authenticated.
- **Scenario** — Open Edit, change values, and save. The system validates and commits them, then shows updated details.
- **Exceptions** — Cancel leaves storage unchanged. Invalid input or a save failure keeps the draft available for correction or retry. A missing Smurf is reported.
- **Postconditions** — A successful edit persists the editable values while preserving creation time and relationships.
- **Requirements** — See the [UC-005 mapping](../_generated/traceability.md#uc-005).

## UC-006 Delete a Smurf

- **Actor and goal** — The operator removes a selected Smurf.
- **Preconditions** — The HTTP caller is authenticated.
- **Scenario** — Select Delete and confirm. The system removes the Smurf and returns to the overview.
- **Exceptions** — Declining confirmation leaves it unchanged. A missing identifier or storage failure is reported.
- **Postconditions** — Successful deletion removes only the selected Smurf.
- **Requirements** — See the [UC-006 mapping](../_generated/traceability.md#uc-006).

## UC-007 Authenticate API access

- **Actor and goal** — The user obtains access to the protected API.
- **Scenario** — Register an account or submit valid email/password credentials. The system returns account information and a bearer token. The client attaches that token to protected requests.
- **Exceptions** — Invalid credentials return 401. Registration errors are reported. Missing or invalid tokens prevent protected requests.
- **Postconditions** — Valid authentication permits access according to the current API policy.
- **Requirements** — See the [UC-007 mapping](../_generated/traceability.md#uc-007).

The client-side logout removes the stored token. Production token lifecycle, authorization roles, and account policies are review items rather than new acceptance claims.

The separate API console realizes UC-007 through interactive sign-in and UC-003 through adjacent pages of five rows. Tokens remain in memory for that process. It also realizes UC-002 with a name-only create prompt. It does not expose registration, edit/delete, filters, or sorting controls. The API validates and commits creation; the client does not automatically retry an unconfirmed create.
