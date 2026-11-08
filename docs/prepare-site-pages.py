"""Adapt local Markdown links for DocFX's published home and API pages."""
import re
from pathlib import Path

docs = Path(__file__).resolve().parent
output = docs / "_generated/site-pages"
output.mkdir(parents=True, exist_ok=True)
home = (docs / "index.md").read_text(encoding="utf-8")
api = (docs / "api-reference.md").read_text(encoding="utf-8")

# DocFX resolves Markdown targets from the generated input directory before
# mapping both pages to the output root.
home = re.sub(
    r"\]\((?!api-reference\.md\))([^:()]+)\)",
    r"](../../\1)",
    home,
)
api = re.sub(r"\]\(_site/api/([\w.]+)\.html\)", r"](xref:\1)", api)
api = api.replace("](documentation.md", "](../../documentation.md")
(output / "index.md").write_text(home, encoding="utf-8", newline="\n")
(output / "api-reference.md").write_text(api, encoding="utf-8", newline="\n")
