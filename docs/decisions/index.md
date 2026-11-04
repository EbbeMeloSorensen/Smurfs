# Architectural decisions

Accepted decisions:

- [ADR 0001: Optional single-village membership without reverse navigation](0001-village-membership.md). Approved by the project owner on 2026-11-04.

The [project context](../project-context.md) records domain intent; the [historical baseline review](../baseline-review.md) retains historical recommendations.

Questions requiring human review:

- Should plain domain objects enforce the accepted single-village membership rule before persistence? The mutable CLR collections currently permit conflicting membership; the relationship cardinality and absence of reverse navigation are already decided in ADR 0001.
- Should DataContext.Configure be wired into model creation? Its key configuration currently matches convention, but future edits to it could remain inactive.
- Should reads keep default tracking or require explicit detached updates? Define concurrency, graph and partial-update expectations first.
- How should external consumers access the internal runtime factory? Define a supported composition/registration surface before widening visibility.
- What domain validation, null-input, missing-row, cancellation and provider transaction guarantees should contracts enforce?

Use the [ADR template](template.md) for a proposal. Record approval explicitly before treating it as an accepted decision. ADR 0001 resolves the relationship cardinality and navigation direction; the questions above remain open.
