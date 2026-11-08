# Smurfs documentation

Smurfs is a .NET 10 reference solution for AI-assisted model-based software engineering. It explores how Codex can maintain engineering artifacts across an iterative lifecycle while the human engineer retains authority over intended behavior and acceptance.

- [Requirements](requirements/index.md) capture intent using FURPS++ and [UML use cases](requirements/use-cases.md).
- [Verification and validation](testing/index.md) describe automated evidence and [manual acceptance](testing/acceptance.md).
- [Engineering process](engineering/index.md) explains the V-model and how the [traceability matrix](_generated/traceability.md) connects intent, design and evidence.

- [Structural](structural/index.md): domain relationships, project boundaries and relational storage.
- [Behavioral](behavioral/index.md): console startup, command and query dispatch, and persistence session behavior.
- [API reference](api/Smurfs.Domain.yml): generated C# signatures and XML contracts; this is not an HTTP API.
- [Decisions](decisions/index.md): questions for review and a template for future approved decisions.

See [getting started](docs/getting-started.md), [documentation publishing](documentation.md), [project context](project-context.md), and the [November 2026 persistence baseline](baseline-2026-11-04.md). The [2026-10-31 review](baseline-review.md) is historical evidence, with corrections linked from the current report.
