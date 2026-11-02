# Documentation architecture and publishing

## Perspectives

The documentation website is organized by viewpoint, not by the capabilities of a particular generator:

| Perspective | Question answered | Primary sources |
| --- | --- | --- |
| Structural | What exists and how is it related/deployed? | PlantUML class/component/deployment models, Markdown |
| Behavioral | How does it interact/change over time? | PlantUML sequence/activity/state models, Markdown |
| API reference | What programming contracts are exposed? | C# signatures and XML documentation, DocFX-generated metadata |
| Decisions | Why did we choose this design? | Human-reviewed Markdown ADRs |

DocFX is the **publisher/assembler**, not the authority for structural and behavioral designs. Its `api/` folder is a generated .NET library API reference, not an HTTP endpoint implementation.

## Authority and consistency

- Implemented public signatures: C# source.
- API descriptions: XML comments checked in beside C#.
- Intended relationships and interactions: reviewed PlantUML models.
- Architectural rationale: Markdown ADRs.
- Published HTML and SVG: derived output, never the canonical model.

Document mismatches rather than silently declaring one source universally authoritative. Label plans and proposals clearly.

## Repository layout

- `docs/structural/` — structural narrative and diagrams.
- `docs/behavioral/` — behavioral narrative and diagrams.
- `docs/decisions/` — ADRs.
- `docs/api/` — generated DocFX metadata.
- `docs/_site/` — generated site.
- `docs/docfx.json`, `docs/toc.yml`, Markdown and `.puml` files — version-controlled inputs.

## Rendering and build

- Prefer separate `.puml` sources, rendered **locally** to SVG; Markdown pages link to those SVG assets. Validate all paths and DocFX resources using the actual configuration.
- Do not duplicate the canonical PlantUML text inside Markdown just to make a diagram render.
- Prefer making diagram generation a documented step before `docfx docfx.json`, with a script reused locally and by CI. Use a project-local tool if already available; otherwise document the chosen PlantUML installation and Java/Graphviz needs.
- Generated assets belong in an explicitly excluded directory, or use narrowly scoped `.gitignore` rules that never accidentally hide hand-authored SVGs.
- DocFX may need `docfx metadata docfx.json` followed by `docfx build docfx.json`; running `docfx docfx.json` normally performs both. Confirm with the installed tool version.
- A documentation build passing means the site was generated; it does **not** prove PlantUML/C# semantic agreement.

## Navigation and linking

- `docs/toc.yml` should expose Home, Structural, Behavioral, API reference and Decisions (without breaking existing links).
- Structural and behavioral Markdown should explain each diagram's scope, not merely display a picture.
- Link to generated API reference using DocFX-supported cross-references when stable; verify those links after generation.
- Empty perspectives can have a short index explaining scope and planned content; do not invent design artifacts.
