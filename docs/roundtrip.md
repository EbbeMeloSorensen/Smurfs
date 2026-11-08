# Roundtrip engineering conventions (reusable)

## Principles

- Source code, design models and explanatory documents are complementary representations, not interchangeable copies.
- Preserve **semantic** consistency: types, interfaces, relationships, cardinalities, behavior and contracts. Do not require identical order or exhaustive listing of implementation details in diagrams.
- For code-first changes, C# establishes implemented behavior; for explicitly model-first work, a reviewed PlantUML proposal establishes design intent. Resolve disagreements explicitly. Never silently overwrite either representation.
- Distinguish `Implemented`, `Proposed` and `Needs reconciliation` descriptions in explanatory text when needed. Do not present proposed design as current implementation.
- Keep design diagrams selective and intelligible; document important public/protected surfaces and relevant associations, not every private helper.

## Requirements and verification

- Use the V-model to pair stakeholder needs with acceptance, system requirements and design with system tests, architecture with integration tests, and component design with unit tests. Iterate across these artifacts as the design changes.
- Classify requirements with FURPS++. Use stable `FR`, `UR`, `RR`, `PR`, `SR`, and `CR` IDs with three-digit numbers, and `ER` for engineering process. Record description, classification, status and verification method. Preserve retired IDs in project history and never assign them to new requirements.
- Model externally meaningful goals as UML use cases with actors and a system boundary. Give each use case a stable ID, scenarios, exceptions and a traceable relationship to requirements.
- Keep intended requirements separate from observed code. Mark proposed intent for review and record the human approval source for material changes.
- Keep verification methods and evidence links current. Document automated scope and gaps, and give manual cases stable IDs, isolated setup, expected results and separate execution records. Manual acceptance requires recorded human execution against the approved baseline.
- Validate trace references during documentation builds. Report unimplemented or uncovered work as a gap and keep the engineering documents consistent with the implemented design.

## Documentation link checks

- Build the documentation with warnings treated as errors. Check affected links from their actual pages in the generated site, including navigation entries, heading anchors, diagrams and API cross-references. Verify that an API link opens the intended HTML page.
- When a page supports local Markdown browsing, check its affected links separately from that view. A successful DocFX build does not establish that a source preview can resolve a cross-reference.
- Report the views checked and any links or targets that could not be verified.

## C# style

- C# files use LF line endings. Git applies this on checkout via `.gitattributes`, and `.editorconfig` sets the same convention for editors. Existing working copies may retain their old line endings until rewritten.
- Follow repository `.editorconfig`: four spaces, Allman braces, file-scoped namespaces and approved naming conventions.
- Preferred member sequence: constants; static fields; instance fields; static properties; instance properties; events; static constructor; instance constructors; public methods; protected methods; internal methods; private methods; nested types. This is a **source convention**, not a UML requirement.
- Separate every pair of adjacent members in handwritten classes, interfaces, structs and records by exactly one blank line, including consecutive properties, fields, constants, events, constructors, methods and nested types. Place the separator before the next member's XML comments and attributes; keep those comments and attributes together with the declaration. Do not add blank lines immediately inside type braces. Leave generated migrations, snapshots and other generated code under their generator's formatting. Standard `dotnet format` does not reliably enforce member separation; inspect it explicitly during review, especially after adding properties or fields.
- Order overloads together and private helpers logically rather than alphabetically. Avoid `#region` unless it materially aids navigation.
- Use precise XML `summary`, `remarks`, `param`, `returns`, `exception` and `see` tags where warranted. Explain behavior, invariants, constraints and non-obvious contracts, not just names. Don't add boilerplate comments to trivial private members.
- In handwritten C#, put each paired documentation block's opening tag, content and closing tag on separate `///` lines. Apply this to summary, remarks, param, typeparam, returns, value, exception and example blocks, even when the text is short. Preserve inline markup such as `<see cref="..." />` and `<c>...</c>` within prose. Keep self-closing tags such as `<inheritdoc />` on one line. Do not insert blank lines between documentation and its member; leave generated code to its generator.

```csharp
/// <summary>
/// Gets the identifier assigned when this instance is constructed.
/// </summary>
public Guid Id { get; private set; } = Guid.NewGuid();
```
- Prefer small, focused changes. CS8618, IDE0005, IDE0161, and IDE0011 are build errors across the solution. Remove unnecessary imports, use file-scoped namespaces, and brace control-flow bodies in handwritten code. Fix the initialization or nullability contract. Use the null-forgiving operator only when an established framework contract guarantees initialization.

## PlantUML conventions

- Commit editable `.puml` sources. Render diagrams for publication as part of a documented build pipeline.
- UML class diagrams distinguish attributes/structural features from operations. A C# property is generally modeled as an attribute; constructor is a creation operation if shown. Keep ER/EF Core mapping diagrams distinct from conceptual class diagrams.
- Use `skinparam classAttributeIconSize 0` where visibility symbols improve readability.
- Annotate important multiplicities, navigability and dependencies; validate them against the actual domain and EF Core configurations.
- Reconcile only the diagrams affected by a change. Omission from a scoped diagram is not evidence a type or relationship should be deleted.
- Diagram generation must not depend on sending private content to a public PlantUML server.

### Assembly architecture views

- Group selected classes and interfaces by their containing production .NET assembly using UML components (`component` with `skinparam componentStyle uml2`). Verify ownership against project files and any AssemblyName overrides; namespaces do not define assembly boundaries. Explain that components represent assemblies.
- Place the most independent domain and contract assemblies toward the top, with dependent implementations below. Use invisible layout constraints where necessary; they express placement only and must not appear as assembly reference arrows. Preserve relationship direction and favor readable routing as the model grows.
- Declare the scope of each architecture view. The persistence architecture view focuses on solution-owned types: omit standalone EF Core/BCL type nodes, shared framework containers and their connecting arrows. Framework types may remain in signatures where useful. Show external types only in a separately scoped view when needed to explain a specific integration.
- Within the declared scope, show inheritance, interface realization, associations and relevant direct type dependencies across component boundaries. Do not add separate assembly/project-reference arrows to this type-oriented view.
- Inspect signatures, generic base classes and interfaces, generic arguments, properties, parameters, return types and configuration targets for relevant dependencies within the view's scope. A framework type appearing in a signature does not require a standalone framework node or arrow. A concrete generic argument creates a dependency even when only named on an inheritance label: for example, `ISmurfRepository : IRepository<Smurf>` warrants `ISmurfRepository ..> Smurf`.
- Add a dashed dependency when a relevant direct type use would otherwise be hidden; label its purpose, such as generic type argument, entity set or configures. Do not duplicate an association, inheritance or realization that already expresses the same type relationship. Do not infer direct dependencies merely from transitive use or a project reference.
- Omit attached comment/note boxes from the persistence architecture diagram. Document API behavior, tracking, query semantics and configuration details in source XML comments published to the API reference. Keep schema/migration status in the relational narrative. A compact notation/scope legend may remain.
- Keep domain types selective, but include those needed as endpoints for the persistence dependencies. Keep domain member details and internal domain associations in the conceptual model.
- As the model grows, check new and changed types against all applicable diagrams. Keep assembly ownership, navigation direction and multiplicities consistent with the inspected code and accepted decisions; document implementation gaps explicitly.

## Change workflow

1. Inspect affected C# and relevant diagrams/docs; identify implemented facts and design intentions.
2. State proposed impact (including public API and persistence/migration implications) before broad changes.
3. Edit code and models together where applicable; update XML comments and conceptual explanations.
4. Verify build, tests, `dotnet format ... --verify-no-changes` (where viable), and documentation build. Naming diagnostics such as IDE1006 may require symbol-aware manual fixes rather than batch auto-fix.
5. Report substantive mismatches for review. Never generate migrations or change schema to 'fix' a diagram without approval.
