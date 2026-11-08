# Engineering process

Smurfs uses the V-model to pair specification and design with evidence. The diagram is a planning model. It does not assert that every artifact or test level already exists.

![Conceptual V-model](../_generated/diagrams/engineering/diagrams/v-model.svg)

[PlantUML source](diagrams/v-model.puml)

Work iteratively. A change can start with a stakeholder need, defect or design discovery. Follow its links through the requirements, use cases, design, code and verification, then revisit any affected assumptions. Plan the corresponding verification while specifying the change. The human engineer approves material changes to intended behavior and decides whether acceptance evidence is sufficient.

**Verification** checks whether the implementation conforms to a specification. **Validation** checks whether the resulting system meets its intended use. A passing SQLite integration test provides verification evidence. Manual acceptance can provide validation evidence when the tester evaluates the workflow against approved requirements and the intended use.

Use the [traceability matrix](../_generated/traceability.md) to traverse these relationships in both directions. Use the [engineering conventions](../roundtrip.md) and [publishing guide](../documentation.md) to maintain the artifacts.
