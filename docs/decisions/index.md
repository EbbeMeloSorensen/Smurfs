# Architectural decisions

No formally approved ADRs were found. The [project context](../project-context.md) records existing intent; the [historical baseline review](../baseline-review.md) retains recommendations, not approved decisions.

Questions requiring human review:

- Should Village membership be exclusive, navigable from Smurf, and persisted? Village is currently public but unmapped; its mutable collection enforces no exclusivity.
- Should DataContext.Configure be wired into model creation? Its key configuration currently matches convention, but future edits to it could remain inactive.
- Should reads keep default tracking or require explicit detached updates? Define concurrency, graph and partial-update expectations first.
- How should external consumers access the internal runtime factory? Define a supported composition/registration surface before widening visibility.
- What domain validation, null-input, missing-row, cancellation and provider transaction guarantees should contracts enforce?

Use the [ADR template](template.md) for a proposal. Record approval explicitly before treating it as an accepted decision. This documentation baseline resolves none of these questions.
