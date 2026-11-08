"""Validate the engineering baseline and generate the DocFX traceability page."""
import json
import re
import sys
from pathlib import Path

DOCS = Path(__file__).resolve().parent
SOURCES = {
    "requirements": ("requirements/index.md", r"(?:FR|UR|RR|PR|SR|CR|ER)-\d{3}"),
    "useCases": ("requirements/use-cases.md", r"UC-\d{3}"),
    "acceptance": ("testing/acceptance.md", r"AT-\d{3}"),
}
CLASSIFICATIONS = {
    "FR": "Functionality",
    "UR": "Usability",
    "RR": "Reliability",
    "PR": "Performance",
    "SR": "Supportability",
    "CR": "++",
    "ER": "Engineering process",
}


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"Duplicate JSON key: {key}")
        result[key] = value
    return result


def definitions(path, pattern):
    titles = {}
    for identifier, title in re.findall(
        rf"^#{{2,3}} ({pattern}) (.+)$", path.read_text(encoding="utf-8"), re.MULTILINE
    ):
        if identifier in titles:
            raise ValueError(f"{path.name}: duplicate definition {identifier}")
        titles[identifier] = title
    return titles


def references(owner, ids, known):
    if not isinstance(ids, list) or any(not isinstance(item, str) for item in ids):
        raise ValueError(f"{owner}: expected a list of reference IDs")
    if len(ids) != len(set(ids)):
        raise ValueError(f"{owner}: repeated reference")
    unknown = set(ids) - known.keys()
    if unknown:
        raise ValueError(f"{owner}: unknown references {', '.join(sorted(unknown))}")


def required_text(identifier, entry, fields):
    for field in fields:
        value = entry.get(field)
        if not isinstance(value, str) or not value.strip():
            raise ValueError(f"{identifier}: missing {field}")


def validate(docs):
    data = json.loads(
        (docs / "requirements/traceability.json").read_text(encoding="utf-8"),
        object_pairs_hook=unique_object,
    )
    titles = {}
    for section, (path, pattern) in SOURCES.items():
        titles[section] = definitions(docs / path, pattern)
        missing_metadata = titles[section].keys() - data[section].keys()
        missing_definition = data[section].keys() - titles[section].keys()
        if missing_metadata or missing_definition:
            raise ValueError(
                f"{section}: missing metadata {sorted(missing_metadata)}, "
                f"missing definitions {sorted(missing_definition)}"
            )

    model = (docs / "requirements/diagrams/use-cases.puml").read_text(encoding="utf-8")
    diagram_ids = re.findall(r'^\s*usecase "(UC-\d{3})\\n', model, re.MULTILINE)
    references("use case diagram", diagram_ids, data["useCases"])
    if set(diagram_ids) != data["useCases"].keys():
        raise ValueError("Use case diagram and specifications contain different IDs")

    requirements = data["requirements"]
    for identifier, entry in requirements.items():
        required_text(identifier, entry, ("classification", "status", "method", "implementation"))
        expected = CLASSIFICATIONS[identifier[:2]]
        actual = entry["classification"]
        if actual != expected and not (expected == "++" and actual.startswith("++ ")):
            raise ValueError(f"{identifier}: wrong classification for its ID")
        if not re.fullmatch(r"Proposed|Approved \(.+\)", entry["status"]):
            raise ValueError(f"{identifier}: expected Proposed or Approved with a source")
        references(identifier, entry["design"], data["design"])
        references(identifier, entry["automated"], data["automated"])

    for identifier, ids in data["useCases"].items():
        references(identifier, ids, requirements)
    for identifier, entry in data["acceptance"].items():
        references(identifier, entry["useCases"], data["useCases"])
        references(identifier, entry["requirements"], requirements)
        realized = {
            requirement for use_case in entry["useCases"] for requirement in data["useCases"][use_case]
        }
        references(identifier, entry["requirements"], dict.fromkeys(realized))

    for identifier, entry in data["design"].items():
        required_text(identifier, entry, ("title", "path", "anchor"))
        headings = re.findall(
            r"^#{1,6} (.+)$", (docs / entry["path"]).read_text(encoding="utf-8"), re.MULTILINE
        )
        if entry["anchor"] not in {slug(heading) for heading in headings}:
            raise ValueError(f"{identifier}: missing design heading {entry['anchor']}")

    for identifier, entry in data["automated"].items():
        if not re.fullmatch(r"(?:IT|VT)-\d{3}", identifier):
            raise ValueError(f"Invalid evidence ID: {identifier}")
        required_text(identifier, entry, ("title", "file", "symbol"))
        source = (docs.parent / entry["file"]).read_text(encoding="utf-8-sig")
        symbol = re.escape(entry["symbol"])
        if not re.search(rf"^\s*(?:public\s+(?:async\s+)?(?:Task|void)|def)\s+{symbol}\s*\(", source, re.MULTILINE):
            raise ValueError(f"{identifier}: missing declaration {entry['symbol']} in {entry['file']}")
    return data, titles


def links(ids):
    return ", ".join(f"[{identifier}](#{identifier.lower()})" for identifier in ids) or "None"



def slug(title):
    return re.sub(r"[^\w -]", "", title.lower()).replace(" ", "-")


def render(data, titles):
    def source_link(section, identifier):
        path, _ = SOURCES[section]
        return f"[{identifier}](../{path}#{identifier.lower()}-{slug(titles[section][identifier])})"

    def row(*cells):
        return "| " + " | ".join(str(cell).replace("|", r"\|").replace("\n", " ") for cell in cells) + " |"

    lines = [
        "# Traceability matrix",
        "",
        "Generated from the checked-in traceability manifest. A link identifies planned or partial "
        "evidence. Consult execution records for actual results.",
        "",
        "## Requirement to design and evidence",
        "",
        "| Requirement | Classification | Status | Verification method | Implementation | Use cases | Design | Automated | Manual | Evidence limits |",
        "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |",
    ]
    for identifier, entry in data["requirements"].items():
        use_cases = [key for key, ids in data["useCases"].items() if identifier in ids]
        manual = [key for key, case in data["acceptance"].items() if identifier in case["requirements"]]
        design = [
            f"[{item['title']}](../{item['path']}#{item['anchor']})"
            for item in (data["design"][key] for key in entry["design"])
        ]
        limits = entry.get("note", "")
        if not entry["automated"] and not manual:
            limits = f"No linked tests. {limits}"
        lines.append(row(
            source_link("requirements", identifier), entry["classification"], entry["status"],
            entry["method"], entry["implementation"], links(use_cases), ", ".join(design) or "None",
            links(entry["automated"]), links(manual), limits,
        ))

    lines.extend(["", "## Use case to requirement"])
    for identifier, ids in data["useCases"].items():
        lines.extend(["", f"### {identifier}", "", f"{source_link('useCases', identifier)} · Requirements {links_to_requirements(ids, source_link)}"])

    lines.extend(["", "## Design to requirement", ""])
    for identifier, entry in data["design"].items():
        ids = [key for key, requirement in data["requirements"].items() if identifier in requirement["design"]]
        lines.append(
            f"- [{entry['title']}](../{entry['path']}#{entry['anchor']}) · "
            f"{links_to_requirements(ids, source_link)}"
        )

    lines.extend(["", "## Manual test to requirement"])
    for identifier, entry in data["acceptance"].items():
        lines.extend([
            "", f"### {identifier}", "",
            f"{source_link('acceptance', identifier)} · Use cases {links(entry['useCases'])} · "
            f"Requirements {links_to_requirements(entry['requirements'], source_link)}",
        ])

    lines.extend(["", "## Automated evidence to requirement"])
    for identifier, entry in data["automated"].items():
        ids = [key for key, requirement in data["requirements"].items() if identifier in requirement["automated"]]
        lines.extend([
            "", f"### {identifier}", "",
            f"{entry['title']} · `{entry['symbol']}` in `{entry['file']}`",
            "", f"Requirements {links_to_requirements(ids, source_link)}",
        ])
    return "\n".join(lines) + "\n"


def links_to_requirements(ids, source_link):
    return ", ".join(source_link("requirements", identifier) for identifier in ids) or "None"


def main():
    try:
        data, titles = validate(DOCS)
        output = DOCS / "_generated/traceability.md"
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(render(data, titles), encoding="utf-8", newline="\n")
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"Traceability error: {error}", file=sys.stderr)
        return 1
    print(f"Validated {len(data['requirements'])} requirements. Generated {output.name}.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
