# Requirements specification

The [use case model](use-cases.md) organizes functional scope. This specification records proposed product intent separately from observed behavior. **Proposed** items need human review. **Approved** items cite an explicit decision or the baseline request. Implementation and coverage evidence are tracked separately in the [traceability matrix](../_generated/traceability.md).

FURPS++ classifies functionality (including business rules and security), usability, reliability, performance, supportability and additional design, implementation, interface or physical constraints. Engineering-process requirements are listed separately. This baseline sets no performance, availability or resource targets. It establishes no standalone usability targets.

## Functionality

### FR-001 Initialize local storage

The operator can initialize an isolated catalog for use. Repeating initialization on an existing catalog preserves its stored Smurfs.

### FR-002 Create a Smurf

The operator can submit a valid Smurf name and, in the WebClient, health and optional birthday, infection status, and gender and receive confirmation that the Smurf was stored and is available in a later session.

### FR-003 List Smurfs

The operator can request a one-based page of stored Smurfs and see the matching rows and the filtered row count. A positive page beyond the available rows shows an empty result.

### FR-004 Validate a proposed Smurf name

The system rejects whitespace-only names and names longer than 20 characters, identifies the reason, and adds no Smurf for a rejected request. The proposed boundary comes from the current domain policy and needs approval as product intent.

### FR-005 Edit a Smurf

The operator can change an existing Smurf's name, health, birthday, infection status, and gender. Invalid values are reported without committing changes. The identifier, creation timestamp, and relationships are preserved.

### FR-006 Delete a Smurf

The operator can delete a selected Smurf. Deletion preserves other Smurfs and related Pet and Village records. A missing identifier is reported.

### FR-007 Filter the catalog

The operator can filter names by case-insensitive literal substring and choose any combination of infected, not infected, and unknown states. Filters combine before counting and paging. Blank name input and no selected infection states include all values.

### FR-008 Order the catalog by name or creation time

The operator can select name ascending or newest first. Ordering applies before paging, with the identifier resolving ties. Each Smurf has a required UTC creation timestamp assigned by the application and preserved by editing. Historical backfill values are documented implementation data.

### FR-009 Inspect a Smurf

The operator can retrieve a selected Smurf's current details, including unknown optional values, and receives a not-found result for a missing identifier.

### FR-010 Authenticate API access

A user can register or sign in to obtain access to protected Smurf API operations. Missing or invalid authentication is rejected. This describes the current local JWT baseline; production authentication policy remains subject to review.

## Non-functional requirements

### Reliability

#### RR-001 Reject invalid paging inputs

The system reports an input error for nonpositive page numbers or page sizes and lets the operator retry with valid values.

### Supportability

#### Diagnosability

##### SR-001 Diagnose failures after exit

After a startup failure or an unexpected application request failure in the console or WPF frontend, a maintainer must be able to read a local diagnostic record after the process exits. The record must identify the frontend, the time of the failure, and the failed startup or application operation, and include exception details when available.

**Approved** by the request to add Diagnosability to the requirements baseline. See the [logging design](../structural/index.md#application-logging) and [manual acceptance case](../testing/acceptance.md#at-007-diagnose-frontend-failures-after-exit).

## Additional constraints

### CR-001 Optional Village membership

A Smurf belongs to at most one Village and may belong to none. Domain navigation goes from Village to Smurfs. The approved relationship is recorded in [ADR 0001](../decisions/0001-village-membership.md). The current CLR collection can represent conflicting membership before persistence, so review enforcement separately.

### CR-002 Restrict deletion of a referenced Pet

The app-data database rejects deletion while stored Smurf references to the Pet remain. EF can clear optional references on tracked Smurfs before deletion. See the approved mapping and scope in [ADR 0002](../decisions/0002-pet-configuration.md).

## Engineering process

### ER-001 Validate traceability on documentation builds

The supported documentation build entry points validate requirement and evidence references and generate a reviewable traceability matrix. Incomplete implementation or test coverage is reported as a gap.

## Decisions for review

Confirm the three console goals, whether explicit initialization is an operator responsibility, the name limit and whitespace policy, and the intended paging contract. Current defaults, ordering, health zero and demo seeding are implementation observations. Decide separately whether those details should become requirements. Village membership and shared Pet relationships exist in the domain and persistence model. The console exposes no Village, Pet or association management workflow, and no user-facing requirement for one has been approved.

FR-005 through FR-010 capture implemented behavior for review and remain Proposed in the traceability matrix. Their addition does not record a formal acceptance decision. Client-specific behavior and current test coverage are linked separately.
