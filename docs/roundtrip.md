# Roundtrip engineering conventions (reusable)

## Principles

- Source code, design models and explanatory documents are complementary representations, not interchangeable copies.
- Preserve **semantic** consistency: types, interfaces, relationships, cardinalities, behavior and contracts. Do not require identical order or exhaustive listing of implementation details in diagrams.
- For code-first changes, C# establishes implemented behavior; for explicitly model-first work, a reviewed PlantUML proposal establishes design intent. Resolve disagreements explicitly. Never silently overwrite either representation.
- Distinguish `Implemented`, `Proposed` and `Needs reconciliation` descriptions in explanatory text when needed. Do not present proposed design as current implementation.
- Keep design diagrams selective and intelligible; document important public/protected surfaces and relevant associations, not every private helper.

## C# style

- Follow repository `.editorconfig`: four spaces, Allman braces, file-scoped namespaces and approved naming conventions.
- Preferred member sequence: constants; static fields; instance fields; static properties; instance properties; events; static constructor; instance constructors; public methods; protected methods; internal methods; private methods; nested types. This is a **source convention**, not a UML requirement.
- Separate adjacent properties, constructors and methods by exactly one blank line; no blank lines immediately inside class braces; group related fields without interstitial blank lines. Standard `dotnet format` may not enforce these spacing rules; check manually.
- Order overloads together and private helpers logically rather than alphabetically. Avoid `#region` unless it materially aids navigation.
- Use precise XML `summary`, `remarks`, `param`, `returns`, `exception` and `see` tags where warranted. Explain behavior, invariants, constraints and non-obvious contracts, not just names. Don't add boilerplate comments to trivial private members.
- Prefer small, focused changes. Treat warnings as errors where the project already does so; don't suppress diagnostics just to make a build green.

## PlantUML conventions

- Commit editable `.puml` sources. Render diagrams for publication as part of a documented build pipeline.
- UML class diagrams distinguish attributes/structural features from operations. A C# property is generally modeled as an attribute; constructor is a creation operation if shown. Keep ER/EF Core mapping diagrams distinct from conceptual class diagrams.
- Use `skinparam classAttributeIconSize 0` where visibility symbols improve readability.
- Annotate important multiplicities, navigability and dependencies; validate them against the actual domain and EF Core configurations.
- Reconcile only the diagrams affected by a change. Omission from a scoped diagram is not evidence a type or relationship should be deleted.
- Diagram generation must not depend on sending private content to a public PlantUML server.

### Assembly architecture views

- Group selected classes and interfaces by their containing production .NET assembly using UML components (`component` with `skinparam componentStyle uml2`). Verify ownership against project files and any AssemblyName overrides; namespaces do not define assembly boundaries. Explain that components represent assemblies.
- Keep external framework types in a clearly labelled area spanning multiple assemblies unless individual external assemblies are relevant to the view.
- Show inheritance, interface realization, associations and direct type dependencies across component boundaries. Do not add separate assembly/project-reference arrows to this type-oriented view.
- Inspect signatures, generic base classes and interfaces, generic arguments, properties, parameters, return types and configuration targets for relevant dependencies. A concrete generic argument creates a dependency even when only named on an inheritance label: for example, `ISmurfRepository : IRepository<Smurf>` warrants `ISmurfRepository ..> Smurf`.
- Add a dashed dependency when a relevant direct type use would otherwise be hidden; label its purpose, such as generic type argument, entity set or configures. Do not duplicate an association, inheritance or realization that already expresses the same type relationship. Do not infer direct dependencies merely from transitive use or a project reference.
- Keep domain types selective, but include those needed as endpoints for the persistence dependencies. Keep domain member details and internal domain associations in the conceptual model.
- As the model grows, check new and changed types against all applicable diagrams. Keep assembly ownership, navigation direction and multiplicities consistent with the inspected code and accepted decisions; document implementation gaps explicitly.

## Change workflow

1. Inspect affected C# and relevant diagrams/docs; identify implemented facts and design intentions.
2. State proposed impact (including public API and persistence/migration implications) before broad changes.
3. Edit code and models together where applicable; update XML comments and conceptual explanations.
4. Verify build, tests, `dotnet format ... --verify-no-changes` (where viable), and documentation build. Naming diagnostics such as IDE1006 may require symbol-aware manual fixes rather than batch auto-fix.
5. Report substantive mismatches for review. Never generate migrations or change schema to 'fix' a diagram without approval.
