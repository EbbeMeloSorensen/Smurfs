# Architectural decisions

Accepted decisions:

- [ADR 0001: Optional single-village membership without reverse navigation](0001-village-membership.md). Approved by the project owner on 2026-11-04.

- [ADR 0002: Explicit Pet mapping during model creation](0002-pet-configuration.md). Approved by the project owner on 2026-11-04.

The [project context](../project-context.md) records domain intent; the [historical baseline review](../baseline-review.md) retains historical recommendations.

Questions requiring human review:

- Should plain domain objects enforce the accepted single-village membership rule before persistence? The mutable CLR collections currently permit conflicting membership; the relationship cardinality and absence of reverse navigation are already decided in ADR 0001.
- Should reads keep default tracking or require explicit detached updates? Define concurrency, graph and partial-update expectations first.
- Which additional domain invariants should be enforced? Smurf name assignment already requires non-whitespace text and at most 20 characters. Null-input, missing-row, cancellation and provider transaction guarantees remain open.

Use the [ADR template](template.md) for a proposal. Record approval explicitly before treating it as an accepted decision. ADR 0001 resolves village cardinality and navigation direction; ADR 0002 records active model configuration and Pet delete behavior; the questions above remain open.
