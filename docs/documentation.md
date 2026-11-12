# Documentation architecture and publishing

## Perspectives

The documentation website is organized by viewpoint, not by the capabilities of a particular generator:

| Perspective | Question answered | Primary sources |
| --- | --- | --- |
| Requirements | What behavior, qualities and constraints are expected? | FURPS++, UML use cases, traceability metadata |
| Verification and validation | How do we verify conformance and validate intended use? | Test plan, manual cases, execution records and evidence links |
| Engineering process | How do we evolve and align engineering artifacts? | V-model, engineering conventions and publishing workflow |
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

- `docs/requirements/` — requirements specification, use cases and traceability manifest.
- `docs/testing/` — test plan, manual acceptance cases and execution record template.
- `docs/engineering/` — conceptual V-model and process overview.
- `docs/structural/` — structural narrative and diagrams.
- `docs/behavioral/` — behavioral narrative and diagrams.
- `docs/decisions/` — ADRs.
- `docs/api/` — generated DocFX metadata.
- `docs/_generated/diagrams/` — generated SVGs preserving source paths relative to `docs/`.
- `docs/_site/` — generated site.
- `docs/build-docs.ps1`, `docs/build-docs.sh` — Windows/PowerShell and Linux/Bash build entry points.
- `docs/docfx.json`, `docs/toc.yml`, Markdown and `.puml` files — version-controlled inputs.

## Rendering and build

- Prefer separate `.puml` sources, rendered **locally** to SVG; Markdown pages link to those SVG assets. Validate all paths and DocFX resources using the actual configuration.
- Do not duplicate the canonical PlantUML text inside Markdown just to make a diagram render.
- Prefer making diagram generation a documented step before `docfx docfx.json`, with a script reused locally and by CI. Use a project-local tool if already available; otherwise document the chosen PlantUML installation and Java/Graphviz needs.
- Generated assets belong in an explicitly excluded directory, or use narrowly scoped `.gitignore` rules that never accidentally hide hand-authored SVGs.
- DocFX may need `docfx metadata docfx.json` followed by `docfx build docfx.json`; running `docfx docfx.json` normally performs both. Confirm with the installed tool version.
- A documentation build passing means the site was generated; it does **not** prove PlantUML/C# semantic agreement.

## Navigation and linking

- `docs/toc.yml` should expose Home, Requirements, Use cases, Structural and Behavioral design, API reference, Verification and validation, Traceability, Engineering process and Decisions (without breaking existing links).
- Structural and behavioral Markdown should explain each diagram's scope, not merely display a picture.
- Link to generated API reference using DocFX-supported cross-references when stable; verify those links after generation.
- Empty perspectives can have a short index explaining scope and planned content; do not invent design artifacts.

## Traceability

Markdown owns requirement titles and descriptions, use case specifications and manual procedures. `requirements/traceability.json` owns classification, approval status, verification methods and relationships. It indexes significant design sections and named automated evidence in source. Build-time generation places the matrix at `_generated/traceability.md`. Edit the sources to update the matrix.

Run `python docs/validate-traceability.py` on Windows or `python3 docs/validate-traceability.py` on Linux. Both build entry points run this check. Duplicate IDs, missing definitions, broken relationships or evidence locations, and missing verification methods fail validation. Empty coverage lists are allowed and the matrix exposes the gaps. The validator checks references and declarations. Test execution and human review establish evidence quality and coverage.

## Local build

Shared prerequisites: .NET SDK supporting net10.0, Python 3.10 or later on PATH, DocFX on PATH, Java and Graphviz `dot` on PATH, and a local PlantUML installation. The build scripts do not download tools or call a rendering service. Use a trusted local JAR or installation. The existing Windows validation used PlantUML bundled with VS Code extension jebbs.plantuml-2.18.1 and DocFX 2.81.0.

### Windows / PowerShell 7

Set `PLANTUML_JAR` or pass `-PlantUmlJar`. From the repository root, run:

```powershell
$env:PLANTUML_JAR = 'C:\path\to\plantuml.jar'
./docs/build-docs.ps1
```

### Linux / Bash

PowerShell is not required. If the `plantuml` command is on PATH, from the repository root run:

```bash
bash docs/build-docs.sh
```

Alternatively, use a local JAR:

```bash
PLANTUML_JAR=/path/to/plantuml.jar bash docs/build-docs.sh
```

Or pass the JAR as the first argument:

```bash
bash docs/build-docs.sh /path/to/plantuml.jar
```

An explicit JAR argument takes precedence over `PLANTUML_JAR`. If neither is set, the Bash script uses `plantuml` on PATH. If a specified JAR is missing, the build fails instead of falling back. For matching rendering across platforms and CI, use the same PlantUML version. The script locates `docs/` relative to itself, so invoking it from another directory also works; relative JAR paths are resolved from the invoking directory.

Both entry points generate the traceability matrix, check documentation `.puml` sources outside `_site`, `_generated` and `api`, render SVGs preserving relative paths under `docs/_generated/diagrams/`, then run `docfx metadata` and `docfx build --warningsAsErrors`. Each external command failure stops the build. The Bash script also checks that each expected SVG was produced, removing only that SVG before rendering so a stale file cannot satisfy the check.

Both build entry points also generate the Projects section. Edit the project descriptions in `docs/project-descriptions.json`. The generator reads project membership from `Smurfs.sln` and direct references and declared versions from the project files. It writes the pages and navigation to `docs/_generated/projects/`.

For example, `docs/structural/diagrams/domain-model.puml` produces `docs/_generated/diagrams/structural/diagrams/domain-model.svg`. Keep the existing Markdown links and `build.resource` mapping to `_generated/diagrams/**`. These scripts render diagrams and build the site; they do not generate `docs/structural/index.md`.

Generated diagrams, metadata and site are ignored; source models, scripts and configuration are tracked. After renaming/removing diagrams, obsolete generated assets can be removed from that dedicated output directory.

After a successful build, serve the output from the repository root:

```bash
docfx serve docs/_site
```

The build entry points also prepare the published home and API landing pages from their Markdown sources. The source API links open local HTML in `_site`, while the published page uses DocFX cross-references.

Running DocFX alone does not perform the separate PlantUML rendering step. Use the platform build entry point whenever diagram sources change or when building a fresh checkout.

Inspect `docs/_site/index.html`, `structural/index.html`, `behavioral/index.html`, `decisions/index.html`, and `api/Smurfs.Domain.Village.html`. API cross-references must resolve to generated pages; diagram links must point to copied SVG resources. A successful build is separate from semantic model review.

When reconciling diagrams, rebuild generated diagrams and the DocFX site in the repository whose documentation the user views. A successful staging-copy build validates the staged inputs but does not refresh that repository's `_generated` or `_site` output. After applying validated source changes, run the repository build entry point there and verify the affected SVG copied into `_site/_generated/diagrams/` contains the expected types and relationships. Report the distinction if only staging validation was possible. Do not edit generated SVG or HTML as the source of a correction.
