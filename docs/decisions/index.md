# Architectural decisions

No formally approved ADRs were found. The [project context](../project-context.md) records existing intent; the [historical baseline review](../baseline-review.md) retains recommendations, not approved decisions.

Questions requiring human review:

- Should Village membership also be enforced in plain domain objects or navigable from Smurf? EF now maps optional membership through a shadow VillageId, while the mutable CLR collection enforces no exclusivity.
- Review the migration needed for Villages, VillageId and the Pet-to-Pets rename. The runtime model has changed; the checked-in migration/snapshot have not.
- Should DataContext.Configure be wired into model creation? Its key configuration currently matches convention, but future edits to it could remain inactive.
- Should reads keep default tracking or require explicit detached updates? Define concurrency, graph and partial-update expectations first.
- How should external consumers access the internal runtime factory? Define a supported composition/registration surface before widening visibility.
- What domain validation, null-input, missing-row, cancellation and provider transaction guarantees should contracts enforce?

Use the [ADR template](template.md) for a proposal. Record approval explicitly before treating it as an accepted decision. This documentation baseline resolves none of these questions.
