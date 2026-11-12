"""Generate the DocFX project catalog from Smurfs.sln and its project files."""
import json
import re
from pathlib import Path
from xml.etree import ElementTree

docs = Path(__file__).resolve().parent
repo = docs.parent
output = docs / "_generated/projects"
descriptions = json.loads((docs / "project-descriptions.json").read_text(encoding="utf-8"))
solution = (repo / "Smurfs.sln").read_text(encoding="utf-8-sig")
project_paths = sorted(
    (repo / path.replace("\\", "/")).resolve()
    for path in re.findall(r'^Project\([^\n]+?\)\s*=\s*"[^"]+",\s*"([^"]+\.csproj)"', solution, re.M)
)
projects = {path: path.stem for path in project_paths}
if not projects or len(set(projects.values())) != len(projects):
    raise SystemExit("Expected solution projects with unique project file names.")

missing = set(projects.values()) - descriptions.keys()
obsolete = descriptions.keys() - set(projects.values())
if missing or obsolete:
    raise SystemExit(f"Project description mismatch. Missing: {sorted(missing)}. Obsolete: {sorted(obsolete)}.")

pages = {}
groups = {"Production projects": [], "Test projects": []}
for path, name in sorted(projects.items(), key=lambda item: item[1].casefold()):
    group = "Test projects" if path.relative_to(repo).parts[0] == "tests" else "Production projects"
    groups[group].append(name)
    project = ElementTree.parse(path).getroot()
    references = sorted(
        projects[(path.parent / ref.attrib["Include"].replace("\\", "/")).resolve()]
        for ref in project.iter("ProjectReference")
        if "Include" in ref.attrib
    )
    packages = sorted(
        (ref.attrib["Include"], ref.get("Version") or ref.findtext("Version"))
        for ref in project.iter("PackageReference")
        if "Include" in ref.attrib
    )
    lines = [f"# {name}", "", descriptions[name], "", f"Project file: `{path.relative_to(repo).as_posix()}`",
             "", "## Direct project references", ""]
    lines.extend([f"- [{reference}]({reference}.md)" for reference in references] or ["None."])
    lines.extend(["", "## Direct NuGet packages", ""])
    if packages:
        lines.extend(["| Package | Declared version |", "| --- | --- |"])
        for package, version in packages:
            declared = f"`{version}`" if version else "See MSBuild configuration"
            lines.append(f"| `{package}` | {declared} |")
    else:
        lines.append("None.")
    pages[f"{name}.md"] = lines

index = ["# Projects", "", "Browse the production and test projects in `Smurfs.sln`.",
         "Each page lists direct references declared in its project file.", ""]
toc = ["- name: Overview", "  href: index.md"]
for group, names in groups.items():
    index.extend([f"## {group}", ""])
    toc.extend([f"- name: {group}", "  items:"])
    for name in names:
        index.append(f"- [{name}]({name}.md) — {descriptions[name]}")
        toc.extend([f"  - name: {name}", f"    href: {name}.md"])
    index.append("")

pages["index.md"] = index
pages["toc.yml"] = toc
output.mkdir(parents=True, exist_ok=True)
for path in output.glob("*.md"):
    if path.name not in pages:
        path.unlink()
for filename, lines in pages.items():
    (output / filename).write_text("\n".join(lines).rstrip() + "\n", encoding="utf-8", newline="\n")
print(f"Generated documentation for {len(projects)} solution projects.")
